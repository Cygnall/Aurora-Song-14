using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Robust.Shared.Utility;

namespace Content.Server.Power.Pow3r
{
    public sealed partial class PowerState
    {
        public static readonly JsonSerializerOptions SerializerOptions = new()
        {
            IncludeFields = true,
            Converters = {new NodeIdJsonConverter()}
        };

        // TODO Aurora Song: probably increase these a bit. benchmark a live server to see what reasonable numbers are
        // TODO Aurora Song: also the values should probably be cvars. are cvars loaded that early?
        public SlotTable<SupplyStruct> Supplies = new(512);
        public SlotTable<LoadStruct> Loads = new(4096);
        public SlotTable<BatteryStruct> Batteries = new(1024);
        public SlotTable<Network> Networks = new(1024);
        public List<List<Network>>? GroupedNets;

        public readonly struct SlotHandle(int index, int generation) : IEquatable<SlotHandle>
        {
            public readonly int Index = index;
            public readonly int Generation = generation;

            public long Combined => (uint) Index | ((long) Generation << 32);

            public SlotHandle(long combined) : this((int) combined, (int) (combined >> 32))
            {
            }

            public bool Equals(SlotHandle other)
            {
                return Index == other.Index && Generation == other.Generation;
            }

            public override bool Equals(object? obj)
            {
                return obj is SlotHandle other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Index, Generation);
            }

            public static bool operator ==(SlotHandle left, SlotHandle right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(SlotHandle left, SlotHandle right)
            {
                return !left.Equals(right);
            }

            public override string ToString()
            {
                return $"{Index} (G{Generation})";
            }
        }

        public static class GenIdStorage
        {
            public static SlotTable<T> FromEnumerable<T>(IEnumerable<(SlotHandle, T)> enumerable)
            {
                return SlotTable<T>.FromEnumerable(enumerable);
            }
        }

        public sealed class SlotTable<T>(int initialCapacity)
        {
            // contiguous values
            private T[] _values = new T[initialCapacity];

            // LSB tracks freed state, so odd numbers = alive, even = freed
            private int[] _generations = new int[initialCapacity];

            private int[] _freeList = new int[initialCapacity];
            private int _freeCount;
            private int _nextId;

            private readonly Lock _structureLock = new();

            public int Count { get; private set; }

            public ref T this[SlotHandle id]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if ((id.Generation & 1) == 0 || (uint)id.Index >= (uint)_generations.Length || _generations[id.Index] != id.Generation)
                        ThrowKeyNotFound();

                    return ref _values[id.Index];
                }
            }

            public ref T GetOrElse(SlotHandle id, ref T other)
            {
                if ((id.Generation & 1) == 0 || (uint)id.Index >= (uint)_generations.Length || _generations[id.Index] != id.Generation)
                    return ref other;

                return ref _values[id.Index];
            }

            public static SlotTable<T> FromEnumerable(IEnumerable<(SlotHandle, T)> enumerable)
            {
                var cache = enumerable.ToArray();

                if (cache.Length == 0)
                    return new SlotTable<T>(0);

                var maxSize = cache.Max(tup => tup.Item1.Index) + 1;

                var storage = new SlotTable<T>(maxSize);

                foreach (var (id, value) in cache)
                {
                    DebugTools.Assert(id.Generation != 0, "Generation cannot be 0");
                    DebugTools.Assert(storage._generations[id.Index] == 0, "Duplicate key index!");
                    DebugTools.Assert((id.Generation & 1) == 1, "Loaded active generation must be odd!");

                    storage._generations[id.Index] = id.Generation;
                    storage._values[id.Index] = value;
                }

                for (var i = 0; i < maxSize; i++)
                {
                    if (storage._generations[i] == 0)
                    {
                        storage._freeList[storage._freeCount++] = i;
                    }
                }

                storage.Count = cache.Length;

                storage._nextId = maxSize;

                DebugTools.Assert(storage.Values.Count == storage.Count);

                return storage;
            }

            public ref T Allocate(out SlotHandle id)
            {
                lock (_structureLock)
                {
                    int index;
                    if (_freeCount > 0)
                    {
                        index = _freeList[--_freeCount];

                        // a recycled slot would be cleared on free if it was a reference
                        // type. otherwise we have to clear it now
                        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                            _values[index] = default!;
                    }
                    else
                    {
                        index = _nextId++;
                        if (index >= _values.Length)
                            Resize();
                    }

                    Count++;

                    int gen = _generations[index] + 1;
                    // if generation counter overflows we should be on 1, not 0
                    if (gen == 0)
                        gen = 1;

                    // increment even -> odd (claimed)
                    _generations[index] = gen;
                    id = new SlotHandle(index, gen);

                    return ref _values[index];
                }
            }

            private void FreeImpl(SlotHandle id)
            {
                if ((uint)id.Index >= (uint)_generations.Length || _generations[id.Index] != id.Generation)
                    ThrowKeyNotFound();

                Count--;

                // increment odd -> even (free)
                _generations[id.Index]++;

                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                    _values[id.Index] = default!;

                if (_freeCount + 1 >= _freeList.Length)
                    ResizeFreeList();

                _freeList[_freeCount++] = id.Index;
            }

            public void Free(SlotHandle id)
            {
                lock (_structureLock)
                {
                    FreeImpl(id);
                }
            }

            public void FreeWithCopyTo(SlotHandle id, ref T other)
            {
                lock (_structureLock)
                {
                    other = _values[id.Index];
                    FreeImpl(id);
                }
            }


            [MethodImpl(MethodImplOptions.NoInlining)]
            private void Resize()
            {
                var newSize = _values.Length * 2;
                Array.Resize(ref _values, newSize);
                Array.Resize(ref _generations, newSize);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private void ResizeFreeList()
            {
                Array.Resize(ref _freeList, _freeList.Length * 2);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void ThrowKeyNotFound()
            {
                throw new KeyNotFoundException();
            }

            public ValuesCollection Values => new(this);

            public readonly struct ValuesCollection(SlotTable<T> owner)
            {
                public int Count => owner.Count;
                public Enumerator GetEnumerator()
                {
                    return new Enumerator(owner);
                }

                public ref struct Enumerator(SlotTable<T> owner)
                {
                    private readonly T[] _values = owner._values;
                    private readonly int[] _generations = owner._generations;
                    private readonly int _maxIndex = owner._nextId;
                    private int _index = -1;

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public bool MoveNext()
                    {
                        while (++_index < _maxIndex)
                        {
                            // skip dead slots
                            if ((_generations[_index] & 1) == 1)
                                return true;
                        }
                        return false;
                    }

                    public ref T Current
                    {
                        [MethodImpl(MethodImplOptions.AggressiveInlining)]
                        get => ref _values[_index];
                    }
                }
            }
        }

        public sealed class NodeIdJsonConverter : JsonConverter<SlotHandle>
        {
            public override SlotHandle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new SlotHandle(reader.GetInt64());
            }

            public override void Write(Utf8JsonWriter writer, SlotHandle value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value.Combined);
            }
        }

        public struct SupplyStruct
        {
            public SupplyStruct() {}

            public SlotHandle Id;

            // == Static parameters ==
            public bool Enabled = true;
            public bool Paused;
            public float MaxSupply;

            public float SupplyRampRate = 5000;
            public float SupplyRampTolerance = 5000;

            // == Runtime parameters ==

            /// <summary>
            ///     Actual power supplied last network update.
            /// </summary>
            public float CurrentSupply;

            /// <summary>
            ///     The amount of power we WANT to be supplying to match grid load.
            /// </summary>
            [JsonIgnore]
            public float SupplyRampTarget;

            /// <summary>
            ///     Position of the supply ramp.
            /// </summary>
            public float SupplyRampPosition;

            [JsonIgnore] public SlotHandle LinkedNetwork;

            /// <summary>
            ///     Supply available during a tick. The actual current supply will be less than or equal to this. Used
            ///     during calculations.
            /// </summary>
            [JsonIgnore] public float AvailableSupply;
        }

        public struct LoadStruct
        {
            public LoadStruct() {}

            public SlotHandle Id;

            // == Static parameters ==
            public bool Enabled = true;
            public bool Paused;
            public float DesiredPower;

            // == Runtime parameters ==
            public float ReceivingPower;

            [JsonIgnore] public SlotHandle LinkedNetwork;
        }

        public struct BatteryStruct
        {
            public BatteryStruct() {}

            public SlotHandle Id;

            // == Static parameters ==
            public bool Enabled = true;
            public bool Paused;
            public bool CanDischarge = true;
            public bool CanCharge = true;
            public float Capacity;
            public float MaxChargeRate;
            public float MaxThroughput; // 0 = infinite cuz imgui
            public float MaxSupply;

            /// <summary>
            ///     The batteries supply ramp tolerance. This is an always available supply added to the ramped supply.
            /// </summary>
            /// <remarks>
            ///     Note that this MUST BE GREATER THAN ZERO, otherwise the current battery ramping calculation will not work.
            /// </remarks>
            public float SupplyRampTolerance = 5000;

            public float SupplyRampRate = 5000;
            public float Efficiency = 1;

            // == Runtime parameters ==
            public float SupplyRampPosition;
            public float CurrentSupply;
            public float CurrentStorage;
            public float CurrentReceiving;
            public float LoadingNetworkDemand;

            [JsonIgnore]
            public bool SupplyingMarked;

            [JsonIgnore]
            public bool LoadingMarked;

            /// <summary>
            ///     Amount of supply that the battery can provide this tick.
            /// </summary>
            [JsonIgnore]
            public float AvailableSupply;

            [JsonIgnore]
            public float DesiredPower;

            [JsonIgnore]
            public float SupplyRampTarget;

            [JsonIgnore]
            public SlotHandle LinkedNetworkCharging;

            [JsonIgnore]
            public SlotHandle LinkedNetworkDischarging;

            /// <summary>
            ///  Theoretical maximum effective supply, assuming the network providing power to this battery continues to supply it
            ///  at the same rate.
            /// </summary>
            public float MaxEffectiveSupply;
        }

        // Readonly breaks json serialization.
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        public sealed class Network
        {
            [ViewVariables] public SlotHandle Id;

            /// <summary>
            ///     Power generators
            /// </summary>
            [ViewVariables] public List<SlotHandle> Supplies = new();

            /// <summary>
            ///     Power consumers.
            /// </summary>
            [ViewVariables] public List<SlotHandle> Loads = new();

            /// <summary>
            ///     Batteries that are draining power from this network (connected to the INPUT port of the battery).
            /// </summary>
            [ViewVariables] public List<SlotHandle> BatteryLoads = new();

            /// <summary>
            ///     Batteries that are supplying power to this network (connected to the OUTPUT port of the battery).
            /// </summary>
            [ViewVariables] public List<SlotHandle> BatterySupplies = new();

            /// <summary>
            ///     The total load on the power network as of last tick.
            /// </summary>
            [ViewVariables] public float LastCombinedLoad = 0f;

            /// <summary>
            ///     Available supply, including both normal supplies and batteries.
            /// </summary>
            [ViewVariables] public float LastCombinedSupply = 0f;

            /// <summary>
            ///     Theoretical maximum supply, including both normal supplies and batteries.
            /// </summary>
            [ViewVariables] public float LastCombinedMaxSupply = 0f;

            [ViewVariables] [JsonIgnore] public int Height;
        }
    }
}
