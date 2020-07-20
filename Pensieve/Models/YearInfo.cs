namespace Pensieve
{
    /// <summary>
    /// Описание года в дневнике
    /// </summary>
    public class YearInfo
    {
        /// <summary>
        /// Номер года
        /// </summary>
        public int Number
        {
            get { return this._Number; }
            set {this._Number=value;}
        }
        private int _Number;

        /// <summary>
        /// Описание года
        /// </summary>
        public string Brief
        {
            get { return this._Brief; }
            set { this._Brief = value; }
        }
        private string _Brief;

        /// <summary>
        /// Код цвета года
        /// </summary>
        public string ColorCode
        {
            get { return this._ColorCode; }
            set { this._ColorCode = value; }
        }
        private string _ColorCode;

        public YearInfo(int Number, string Brief, string ColorCode)
        {
            this._Number = Number;
            this._Brief = Brief;
            this._ColorCode = ColorCode;
        }
    }
}