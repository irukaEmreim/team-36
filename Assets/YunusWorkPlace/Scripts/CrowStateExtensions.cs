


using static CrowController; // CrowState enumu için

public static class CrowStateExtensions
{
    public static bool IsGrounded(this CrowState state)
    {
        return state == CrowState.Idle || state == CrowState.GroundMovement;
    }

    public static bool IsFlying(this CrowState state)
    {
        return state == CrowState.Flight;
    }

    public static bool IsAttacking(this CrowState state)
    {
        return state == CrowState.DirectAttack;
    }
}
