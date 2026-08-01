using System.Runtime.CompilerServices;
using Robust.Shared.Configuration;

namespace Content.Server._AS;

public sealed class TickLimiter(EntitySystem.Subscriptions subs, CVarDef<int> cVar, bool discrete=false)
{
    private bool _initialized = false;
    private int _everyN = 0; // every tick by default
    private int _ticksPassed = 0;
    private float _tickTimeAcc = 0;

    public void UpdateTickLimit(int limit)
    {
        _everyN = limit;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float CheckTickLimit(float tickTime)
    {
        if (!_initialized)
        {
            subs.CVar(IoCManager.Resolve<IConfigurationManager>(), cVar, UpdateTickLimit);
            _initialized = true;
        }

        if (_everyN <= 1)
            return tickTime;

        _tickTimeAcc += tickTime;
        _ticksPassed++;

        if (_ticksPassed < _everyN)
            return 0f;

        var ret = _tickTimeAcc;
        _tickTimeAcc = 0f;
        _ticksPassed = 0;
        return discrete ? tickTime : ret;
    }
}
