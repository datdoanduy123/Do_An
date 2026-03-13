namespace Domain.Enums
{
    public enum TrangThaiDuAn
    {
        Planning = 0,
        Active = 1,
        Completed = 2,
        Cancelled = 3
    }

    public enum TrangThaiSprint
    {
        New = 0,
        InProgress = 1,
        Finished = 2
    }

    public enum TrangThaiCongViec
    {
        Todo = 0,
        InProgress = 1,
        Review = 2,
        Done = 3,
        Cancelled = 4
    }

    public enum DoUuTien
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Urgent = 3
    }

    public enum LoaiCongViec
    {
        Backend = 0,
        Frontend = 1,
        Fullstack = 2,
        Mobile = 3,
        DevOps = 4,
        Tester = 5,
        UIUX = 6,
        BA = 7
    }

    public enum PhuongThucGiaoViec
    {
        Manual = 0,
        AI = 1
    }
}
