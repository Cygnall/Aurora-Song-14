using System.Text.Json.Serialization;
using Content.Server.Power.EntitySystems;

namespace Content.Server.Power.Pow3r
{
    // TODO: one allocated to a pow3r instance the backing initialization struct isn't necessary
    //   we should have a reasonably sized object pool for each type and claim/release them
    //   otherwise we're duplicating RAM unnecessarily

    /*
     * Because we've moved PowerState.SlotHandle and its related internal structures to contiguous
     * value types, there is an architectural incompatibility with components like PowerConsumerComponent
     * which directly reference what were formerly value types.
     *
     * Rather than refactor hundreds of uses of those components now and in the future, we instead
     * duplicate ref type semantics for the ECS system while retaining underlying value type semantics
     * for the hot path in power solvers.
     *
     * Each ref type instance allocates a struct, which has appropriate initial values, and may be
     * mutated prior to being associated with a power net.
     */
    public sealed partial class PowerState
    {
        public sealed class Supply
        {
            public Supply() {}

            [ViewVariables] public SlotHandle Id;

            private SupplyStruct _initializationStruct = new ();
            public ref SupplyStruct InitializationStruct => ref _initializationStruct;
            public PowerState? PowerStateInst = null;

            private ref SupplyStruct Inst()
            {
                if (PowerStateInst != null)
                    return ref PowerStateInst.Supplies.GetOrElse(Id, ref _initializationStruct);

                return ref _initializationStruct;
            }

            // == Static parameters ==
            [ViewVariables(VVAccess.ReadWrite)]
            public bool Enabled
            {
                get => Inst().Enabled;
                set => Inst().Enabled = value;
            }
            [ViewVariables(VVAccess.ReadWrite)]
            public bool Paused
            {
                get => Inst().Paused;
                set => Inst().Paused = value;
            }
            [ViewVariables(VVAccess.ReadWrite)]
            public float MaxSupply
            {
                get => Inst().MaxSupply;
                set => Inst().MaxSupply = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public float SupplyRampRate
            {
                get => Inst().SupplyRampRate;
                set => Inst().SupplyRampRate = value;
            }
            [ViewVariables(VVAccess.ReadWrite)]
            public float SupplyRampTolerance
            {
                get => Inst().SupplyRampTolerance;
                set => Inst().SupplyRampTolerance = value;
            }

            // == Runtime parameters ==

            /// <summary>
            ///     Actual power supplied last network update.
            /// </summary>
            [ViewVariables(VVAccess.ReadWrite)]
            public float CurrentSupply
            {
                get => Inst().CurrentSupply;
                set => Inst().CurrentSupply = value;
            }

            /// <summary>
            ///     The amount of power we WANT to be supplying to match grid load.
            /// </summary>
            [ViewVariables(VVAccess.ReadWrite)]
            [JsonIgnore]
            public float SupplyRampTarget
            {
                get => Inst().SupplyRampTarget;
                set => Inst().SupplyRampTarget = value;
            }

            /// <summary>
            ///     Position of the supply ramp.
            /// </summary>
            [ViewVariables(VVAccess.ReadWrite)]
            public float SupplyRampPosition
            {
                get => Inst().SupplyRampPosition;
                set => Inst().SupplyRampPosition = value;
            }

            [ViewVariables]
            [JsonIgnore]
            public SlotHandle LinkedNetwork
            {
                get => Inst().LinkedNetwork;
                set => Inst().LinkedNetwork = value;
            }

            /// <summary>
            ///     Supply available during a tick. The actual current supply will be less than or equal to this. Used
            ///     during calculations.
            /// </summary>
            [JsonIgnore]
            public float AvailableSupply
            {
                get => Inst().AvailableSupply;
                set => Inst().AvailableSupply = value;
            }
        }

        public sealed class Load
        {
            public Load() { }

            [ViewVariables] public SlotHandle Id;

            private LoadStruct _initializationStruct = new ();
            public ref LoadStruct InitializationStruct => ref _initializationStruct;
            public PowerState? PowerStateInst = null;

            private ref LoadStruct Inst()
            {
                if (PowerStateInst != null)
                    return ref PowerStateInst.Loads.GetOrElse(Id, ref _initializationStruct);

                return ref _initializationStruct;
            }

            // == Static parameters ==
            [ViewVariables(VVAccess.ReadWrite)]
            public bool Enabled
            {
                get => Inst().Enabled;
                set => Inst().Enabled = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public bool Paused
            {
                get => Inst().Paused;
                set => Inst().Paused = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public float DesiredPower
            {
                get => Inst().DesiredPower;
                set => Inst().DesiredPower = value;
            }

            // == Runtime parameters ==
            [ViewVariables(VVAccess.ReadWrite)]
            public float ReceivingPower
            {
                get => Inst().ReceivingPower;
                set => Inst().ReceivingPower = value;
            }

            [ViewVariables] [JsonIgnore] public SlotHandle LinkedNetwork
            {
                get => Inst().LinkedNetwork;
                set => Inst().LinkedNetwork = value;
            }
        }

        public sealed class Battery
        {
            public Battery() { }

            [ViewVariables] public SlotHandle Id;

            private BatteryStruct _initializationStruct = new ();
            public ref BatteryStruct InitializationStruct => ref _initializationStruct;
            public PowerState? PowerStateInst = null;

            private ref BatteryStruct Inst()
            {
                if (PowerStateInst != null)
                    return ref PowerStateInst.Batteries.GetOrElse(Id, ref _initializationStruct);

                return ref _initializationStruct;
            }

            // == Static parameters ==
            [ViewVariables(VVAccess.ReadWrite)]
            public bool Enabled
            {
                get => Inst().Enabled;
                set => Inst().Enabled = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public bool Paused
            {
                get => Inst().Paused;
                set => Inst().Paused = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public bool CanDischarge
            {
                get => Inst().CanDischarge;
                set => Inst().CanDischarge = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public bool CanCharge
            {
                get => Inst().CanCharge;
                set => Inst().CanCharge = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public float Capacity
            {
                get => Inst().Capacity;
                set => Inst().Capacity = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public float MaxChargeRate
            {
                get => Inst().MaxChargeRate;
                set => Inst().MaxChargeRate = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public float MaxThroughput
            {
                get => Inst().MaxThroughput;
                set => Inst().MaxThroughput = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public float MaxSupply
            {
                get => Inst().MaxSupply;
                set => Inst().MaxSupply = value;
            }

            /// <summary>
            ///     The batteries supply ramp tolerance. This is an always available supply added to the ramped supply.
            /// </summary>
            /// <remarks>
            ///     Note that this MUST BE GREATER THAN ZERO, otherwise the current battery ramping calculation will not work.
            /// </remarks>
            [ViewVariables(VVAccess.ReadWrite)]
            public float SupplyRampTolerance
            {
                get => Inst().SupplyRampTolerance;
                set => Inst().SupplyRampTolerance = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public float SupplyRampRate
            {
                get => Inst().SupplyRampRate;
                set => Inst().SupplyRampRate = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public float Efficiency
            {
                get => Inst().Efficiency;
                set => Inst().Efficiency = value;
            }

            // == Runtime parameters ==
            [ViewVariables(VVAccess.ReadWrite)]
            public float SupplyRampPosition
            {
                get => Inst().SupplyRampPosition;
                set => Inst().SupplyRampPosition = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public float CurrentSupply
            {
                get => Inst().CurrentSupply;
                set => Inst().CurrentSupply = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public float CurrentStorage
            {
                get => Inst().CurrentStorage;
                set => Inst().CurrentStorage = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public float CurrentReceiving
            {
                get => Inst().CurrentReceiving;
                set => Inst().CurrentReceiving = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            public float LoadingNetworkDemand
            {
                get => Inst().LoadingNetworkDemand;
                set => Inst().LoadingNetworkDemand = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            [JsonIgnore]
            public bool SupplyingMarked
            {
                get => Inst().SupplyingMarked;
                set => Inst().SupplyingMarked = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            [JsonIgnore]
            public bool LoadingMarked
            {
                get => Inst().LoadingMarked;
                set => Inst().LoadingMarked = value;
            }

            /// <summary>
            ///     Amount of supply that the battery can provide this tick.
            /// </summary>
            [ViewVariables(VVAccess.ReadWrite)]
            [JsonIgnore]
            public float AvailableSupply
            {
                get => Inst().AvailableSupply;
                set => Inst().AvailableSupply = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            [JsonIgnore]
            public float DesiredPower
            {
                get => Inst().DesiredPower;
                set => Inst().DesiredPower = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            [JsonIgnore]
            public float SupplyRampTarget
            {
                get => Inst().SupplyRampTarget;
                set => Inst().SupplyRampTarget = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            [JsonIgnore]
            public SlotHandle LinkedNetworkCharging
            {
                get => Inst().LinkedNetworkCharging;
                set => Inst().LinkedNetworkCharging = value;
            }

            [ViewVariables(VVAccess.ReadWrite)]
            [JsonIgnore]
            public SlotHandle LinkedNetworkDischarging
            {
                get => Inst().LinkedNetworkDischarging;
                set => Inst().LinkedNetworkDischarging = value;
            }

            /// <summary>
            ///  Theoretical maximum effective supply, assuming the network providing power to this battery continues to supply it
            ///  at the same rate.
            /// </summary>
            [ViewVariables]
            public float MaxEffectiveSupply
            {
                get => Inst().MaxEffectiveSupply;
                set => Inst().MaxEffectiveSupply = value;
            }
        }
    }
}
