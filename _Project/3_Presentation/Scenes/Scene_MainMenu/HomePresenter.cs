// Assets/_Project/3_Presentation/Scene_MainMenu/Scripts/HomePresenter.cs
using Billiards.Presentation;
using VContainer;

public class HomePresenter
{
    private readonly PlayerSession _session;
    private readonly HomeMenu _view;

    // VContainer injects both the data and the UI reference
    [Inject]
    public HomePresenter(PlayerSession session, HomeMenu view)
    {
        _session = session;
        _view = view;
    }

    public void Initialize()
    {
        var profile = _session.CurrentProfile;

        // Pass generic data to the dumb view
        _view.UpdateDisplay(
            string.IsNullOrEmpty(profile.DisplayName) ? "Guest" : profile.DisplayName,
            profile.Level,
            profile.CountryCode
        );
    }
}