public struct DayInfo
{
    private readonly int _Num;
    private readonly string _Title;
    private readonly bool _IsImportant;

    public int Num { get { return this._Num; } }
    public string Title { get { return this._Title; } }
    public bool IsImportant { get { return this._IsImportant; } }

    public DayInfo(int Num, string Title, bool IsImportant)
    {
        this._Num = Num;
        this._Title = Title;
        this._IsImportant = IsImportant;
    }
}