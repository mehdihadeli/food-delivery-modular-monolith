namespace BuildingBlocks.Abstractions.CQRS.Query;

public interface IListQuery<out TResponse> : IPageRequest, IQuery<TResponse>
    where TResponse : notnull
{
}
