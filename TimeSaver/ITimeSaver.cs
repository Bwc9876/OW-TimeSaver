namespace TimeSaver;

public interface ITimeSaver
{
    public void ToggleSkipSplash(bool? enabled);
    public void ToggleSkipDeathSequence(bool? enabled);
    public void ToggleSkipPopups(bool? enabled);
    public void ToggleSkipCredits(bool? enabled);
    public void ToggleSkipPostCredits(bool? enabled);
    public void ToggleAlwaysStartWithSuit(bool? enabled);
    public void ToggleSuppressSlate(bool? enabled);
}
