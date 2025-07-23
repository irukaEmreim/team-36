


using static Crow_MainController; // CrowState enumu için

public static class Crow_StateExtensions
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

    public static bool IsCarryingItem(this CrowState state)
    {
        return state == CrowState.CarryingItem;
    }
}
