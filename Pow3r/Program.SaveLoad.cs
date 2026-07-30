using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Content.Server.Power.Pow3r;
using static Content.Server.Power.Pow3r.PowerState;

namespace Pow3r
{
    internal sealed partial class Program
    {
        private void LoadFromDisk()
        {
            if (!File.Exists("data.json"))
                return;

            var dat = JsonSerializer.Deserialize<DiskDat>(File.ReadAllBytes("data.json"), SerializerOptions);

            if (dat == null)
                return;

            _paused = dat.Paused;
            _currentSolver = dat.Solver;

            _state = new PowerState
            {
                Networks = GenIdStorage.FromEnumerable(dat.Networks.Select(n => (n.Id, n))),
                Supplies = GenIdStorage.FromEnumerable(dat.Supplies.Select(s => (s.Id, PreallocBackingStruct: s.InitializationStruct))),
                Loads = GenIdStorage.FromEnumerable(dat.Loads.Select(l => (l.Id, PreallocBackingStruct: l.InitializationStruct))),
                Batteries = GenIdStorage.FromEnumerable(dat.Batteries.Select(b => (b.Id, PreallocBackingStruct: b.InitializationStruct)))
            };

            _displayLoads = dat.Loads.ToDictionary(n => n.Id, _ => new DisplayLoad());
            _displaySupplies = dat.Supplies.ToDictionary(n => n.Id, _ => new DisplaySupply());
            _displayBatteries = dat.Batteries.ToDictionary(n => n.Id, _ => new DisplayBattery());
            _displayNetworks = dat.Networks.ToDictionary(n => n.Id, _ => new DisplayNetwork());

            RefreshLinks();
        }

        private delegate TReturn RefFunc<TSlot, out TReturn>(ref TSlot item);

        private List<TReturn> MapSlots<TSlot, TReturn>(SlotTable<TSlot> slots, RefFunc<TSlot, TReturn> mapper)
        {
            var list = new List<TReturn>(slots.Values.Count);
            var enumerator = slots.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                list.Add(mapper(ref enumerator.Current));
            }

            return list;
        }

        private void SaveToDisk()
        {
            var data = new DiskDat
            {
                Paused = _paused,
                Solver = _currentSolver,

                Loads = MapSlots(_state.Loads,
                    (ref item) =>
                {
                    var ret = new Load
                    {
                        Id = item.Id,
                        InitializationStruct = item,
                    };
                    return ret;
                }),
                Batteries = MapSlots(_state.Batteries,
                    (ref item) =>
                {
                    var ret = new Battery
                    {
                        Id = item.Id,
                        InitializationStruct = item,
                    };
                    return ret;
                }),
                Networks = MapSlots(_state.Networks,
                (ref item) => item),
                Supplies = MapSlots(_state.Supplies,
                    (ref item) =>
                {
                    var ret = new Supply
                    {
                        Id = item.Id,
                        InitializationStruct = item,
                    };
                    return ret;
                }),
            };

            File.WriteAllBytes("data.json", JsonSerializer.SerializeToUtf8Bytes(data, SerializerOptions));
        }

        private sealed class DiskDat
        {
            public bool Paused;
            public int Solver;

            public List<Load> Loads;
            public List<Network> Networks;
            public List<Supply> Supplies;
            public List<Battery> Batteries;
        }
    }
}
