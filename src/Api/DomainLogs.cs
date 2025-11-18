namespace Api
{
    public static partial class StockLogs
    {
        [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Stock {Symbol} created by User:{UserId}")]
        public static partial void StockCreated(ILogger logger, string symbol, string userId);

        [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Stock Id:{Id} updated by User:{UserId}")]
        public static partial void StockUpdated(ILogger logger, int id, string userId);

        [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Stock Id:{Id} deleted by User:{UserId}")]
        public static partial void StockDeleted(ILogger logger, int id, string userId);

        [LoggerMessage(EventId = 1004, Level = LogLevel.Warning, Message = "Stock Id:{Id} not found")]
        public static partial void StockNotFound(ILogger logger, int id);

        [LoggerMessage(EventId = 1005, Level = LogLevel.Warning, Message = "GetBySymbol failed. Symbol:{Symbol} not found")]
        public static partial void StockSymbolNotFound(ILogger logger, string symbol);

        [LoggerMessage(EventId = 1006, Level = LogLevel.Warning, Message = "GetBySymbol called with empty symbol")]
        public static partial void StockSymbolEmpty(ILogger logger);

        [LoggerMessage(EventId = 1007, Level = LogLevel.Information, Message = "Stock query executed. Total:{Total} ElapsedMs:{ElapsedMs}")]
        public static partial void StockQueryExecuted(ILogger logger, int total, double elapsedMs);
    }

    public static partial class CommentLogs
    {
        [LoggerMessage(EventId = 1101, Level = LogLevel.Information, Message = "Comment Id:{CommentId} created by User:{UserId}")]
        public static partial void CommentCreated(ILogger logger, int commentId, string userId);

        [LoggerMessage(EventId = 1102, Level = LogLevel.Information, Message = "Comment Id:{Id} updated by User:{UserId}")]
        public static partial void CommentUpdated(ILogger logger, int id, string userId);

        [LoggerMessage(EventId = 1103, Level = LogLevel.Information, Message = "Comment Id:{Id} deleted by User:{UserId}")]
        public static partial void CommentDeleted(ILogger logger, int id, string userId);

        [LoggerMessage(EventId = 1104, Level = LogLevel.Warning, Message = "Comment Id:{Id} not found")]
        public static partial void CommentNotFound(ILogger logger, int id);

        [LoggerMessage(EventId = 1105, Level = LogLevel.Warning, Message = "Comment Id:{CommentId} forbidden for User:{UserId}")]
        public static partial void CommentForbidden(ILogger logger, int commentId, string userId);

        [LoggerMessage(EventId = 1106, Level = LogLevel.Information, Message = "Comments listed Total:{Total} ElapsedMs:{ElapsedMs}")]
        public static partial void CommentList(ILogger logger, int total, double elapsedMs);
    }

    public static partial class PortfolioLogs
    {
        [LoggerMessage(EventId = 1201, Level = LogLevel.Information, Message = "Portfolio fetched for User:{UserId}")]
        public static partial void PortfolioFetched(ILogger logger, string userId);

        [LoggerMessage(EventId = 1202, Level = LogLevel.Warning, Message = "Add to Portfolio failed. Stock Symbol:{Symbol} not found for User:{UserId}")]
        public static partial void AddStockNotFound(ILogger logger, string symbol, string userId);

        [LoggerMessage(EventId = 1203, Level = LogLevel.Information, Message = "Stock {Symbol} added to Portfolio for User:{UserId}")]
        public static partial void PortfolioStockAdded(ILogger logger, string symbol, string userId);

        [LoggerMessage(EventId = 1204, Level = LogLevel.Warning, Message = "Remove from Portfolio failed. Stock Symbol:{Symbol} not found in Portfolio for User:{UserId}")]
        public static partial void RemoveNotFound(ILogger logger, string symbol, string userId);

        [LoggerMessage(EventId = 1205, Level = LogLevel.Information, Message = "Stock {Symbol} removed from Portfolio for User:{UserId}")]
        public static partial void PortfolioStockRemoved(ILogger logger, string symbol, string userId);

        [LoggerMessage(EventId = 1206, Level = LogLevel.Warning, Message = "Add to Portfolio skipped. Stock Symbol:{Symbol} already exists for User:{UserId}")]
        public static partial void AddDuplicate(ILogger logger, string symbol, string userId);
    }
}