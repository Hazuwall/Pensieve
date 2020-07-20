using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Popups;

namespace Pensieve
{
    public static class UIHelper
    {
        /// <summary>
        /// Получить цвет, соотвествующий Hex-коду
        /// </summary>
        /// <param name="HexCode">Hex-код</param>
        /// <exception cref="System.FormatException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <returns></returns>
        public static Color GetColorFromHex(string HexCode)
        {
            if (HexCode == null)
                throw new ArgumentNullException();
            else if (!HexCode[0].Equals('#') || HexCode.Length != 7)
                throw new FormatException();
            byte R = Byte.Parse(HexCode.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            byte G = Byte.Parse(HexCode.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            byte B = Byte.Parse(HexCode.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            return Color.FromArgb(255, R, G, B);
        }

        public static IAsyncOperation<IUICommand> ShowWarningDialogAsync(UICommandInvokedHandler Callback)
        {
            MessageDialog MessageBox = new MessageDialog("Вы уверены, что хотите прододолжить?", "Внимание!");
            MessageBox.Commands.Add(new UICommand("Продолжить", Callback));
            MessageBox.Commands.Add(new UICommand("Отмена"));
            MessageBox.CancelCommandIndex = 1;
            MessageBox.DefaultCommandIndex = 1;
            return MessageBox.ShowAsync();
        }
        public static IAsyncOperation<IUICommand> ShowMessageDialogAsync(string Message)
        {
            MessageDialog MessageBox = new MessageDialog(Message,"Сообщение");
            MessageBox.Commands.Add(new UICommand("Хорошо"));
            MessageBox.CancelCommandIndex = 0;
            MessageBox.DefaultCommandIndex = 0;
            return MessageBox.ShowAsync();
        }
    }
}
