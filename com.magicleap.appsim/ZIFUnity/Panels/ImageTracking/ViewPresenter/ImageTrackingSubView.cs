namespace MagicLeap.ZI
{
    internal class ImageTrackingSubView : ObjectView<ImageTargetBuilderWrapper>
    {
        protected ImageTrackingSubView(ImageTargetBuilderWrapper bondedObject) : base(bondedObject)
        {
        }

        public void Init()
        {
            Initialize();
        }

        public void SynchronizeStateExposer()
        {
            SynchronizeViewWithState();
        }
    }
}
