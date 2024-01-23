using MobX.Utilities;
using TMPro;

namespace MobX.Mediator.Pooling
{
    public class TextPool : PoolAsset<TMP_Text>
    {
        protected override void OnReleaseInstance(TMP_Text instance)
        {
            instance.SetActive(false);

            instance.transform.SetParent(Parent);
        }

        protected override void OnGetInstance(TMP_Text instance)
        {
            instance.SetActive(true);
        }
    }
}