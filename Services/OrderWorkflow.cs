using WudilFood.Models;
namespace WudilFood.Services;

public static class OrderWorkflow
{
    public static OrderState? Next(OrderState current, Fulfillment fulfillment) => current switch
    {
        OrderState.Pending or OrderState.Accepted => OrderState.Processing,
        OrderState.Processing => OrderState.Ready,
        OrderState.Ready when fulfillment == Fulfillment.Delivery => OrderState.Shipped,
        OrderState.Ready => OrderState.Completed,
        OrderState.Shipped => OrderState.Delivered,
        OrderState.Delivered => OrderState.Completed,
        _ => null
    };

    public static bool CanCancel(OrderState current) =>
        current is OrderState.Pending or OrderState.Accepted or OrderState.Processing;

    public static bool CanTransition(OrderState current, OrderState next, Fulfillment fulfillment) =>
        Next(current, fulfillment) == next || (next == OrderState.Cancelled && CanCancel(current));

    public static void RestoreStock(FoodOrder order)
    {
        foreach(var line in order.Lines)
            line.MenuItem.Stock += line.Quantity;
    }

    public static IReadOnlyList<OrderState> Timeline(Fulfillment fulfillment) =>
        fulfillment == Fulfillment.Delivery
            ? new[] { OrderState.Pending, OrderState.Processing, OrderState.Ready, OrderState.Shipped, OrderState.Delivered, OrderState.Completed }
            : new[] { OrderState.Pending, OrderState.Processing, OrderState.Ready, OrderState.Completed };

    public static string ActionLabel(OrderState next) => next switch
    {
        OrderState.Processing => "Start processing",
        OrderState.Ready => "Mark ready",
        OrderState.Shipped => "Mark shipped",
        OrderState.Delivered => "Mark delivered",
        OrderState.Completed => "Complete order",
        _ => "Update order"
    };
}

