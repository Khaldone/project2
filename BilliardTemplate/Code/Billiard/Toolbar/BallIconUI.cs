using UnityEngine;
using UnityEngine.UI;

namespace ibc.game
{
    public class BallIconUI : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMPro.TMP_Text _label;
        [SerializeField] private GameObject[] _stripes;

        public void Setup(Color color, BallType ballType, int ballId)
        {
            foreach (var stripe in _stripes)
            {
                stripe.gameObject.SetActive(ballType == BallType.Stripe);
            }


            if (_iconImage != null)
                _iconImage.color = color;

            if (_label != null)
                _label.text = ballId.ToString();
        }
    }
}