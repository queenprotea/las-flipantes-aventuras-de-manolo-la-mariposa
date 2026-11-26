using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class ForfeitSection
    {
        private readonly ForfeitFlow _forfeitFlow;
        private readonly ForfeitView _forfeitView;

        internal ForfeitSection(ForfeitFlow forfeitFlow, string forfeitButtonKey)
        {
            _forfeitFlow = forfeitFlow;
            _forfeitView = new ForfeitView(forfeitButtonKey);
        }

        internal Widget Root => _forfeitView.Root;

        internal void Reset()
        {
            _forfeitFlow.Forget();
            _forfeitView.Reset();
        }

        internal ScreenId? TakeNextScreen()
        {
            if (_forfeitView.TakeConfirmation())
            {
                _forfeitFlow.Forfeit();
            }

            _forfeitView.ShowWaiting(_forfeitFlow.IsForfeiting);
            ScreenOutcome? outcome = _forfeitFlow.TakeOutcome();
            if (outcome?.Notice is not null)
            {
                _forfeitView.ShowServerError();
            }
            else if (outcome is not null)
            {
                _forfeitView.HideQuestion();
            }

            ScreenId? nextScreen = outcome?.NextScreen;

            return nextScreen;
        }
    }
}
