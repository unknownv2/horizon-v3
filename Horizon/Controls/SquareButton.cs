using DevComponents.DotNetBar;

namespace NoDev.Horizon.Controls
{
    sealed class SquareButton : ButtonX
    {
        public SquareButton()
        {
            this.FocusCuesEnabled = false;
            this.Shape = new RoundRectangleShapeDescriptor();
        }
    }
}
