using TMPro;
using UnityEngine;

public sealed class TransportAmountInputUI : MonoBehaviour
{
    [SerializeField] private TransportRouteBuilder routeBuilder;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private int defaultAmount = 1;

    private void Start()
    {
        if (routeBuilder == null || inputField == null)
        {
            Debug.LogError("TransportAmountInputUI missing references.");
            return;
        }

        inputField.text = defaultAmount.ToString();
        routeBuilder.SetAmountPerTransfer(defaultAmount);
        inputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnEndEdit(string value)
    {
        if (!int.TryParse(value, out int amount))
            amount = defaultAmount;

        amount = Mathf.Max(1, amount);

        inputField.text = amount.ToString();
        routeBuilder.SetAmountPerTransfer(amount);
    }
}