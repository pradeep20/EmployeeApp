namespace EmployeeAPI.Models
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public int TotalRecords { get; set; }
    }
}
