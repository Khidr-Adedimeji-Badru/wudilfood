using WudilFood.Models;
using WudilFood.Services;
using Xunit;
namespace WudilFood.Tests;

public class OrderWorkflowTests
{
    [Theory]
    [InlineData(OrderState.Pending, Fulfillment.Delivery, OrderState.Processing)]
    [InlineData(OrderState.Processing, Fulfillment.Delivery, OrderState.Ready)]
    [InlineData(OrderState.Ready, Fulfillment.Delivery, OrderState.Shipped)]
    [InlineData(OrderState.Shipped, Fulfillment.Delivery, OrderState.Delivered)]
    [InlineData(OrderState.Delivered, Fulfillment.Delivery, OrderState.Completed)]
    [InlineData(OrderState.Ready, Fulfillment.SelfPickup, OrderState.Completed)]
    [InlineData(OrderState.Ready, Fulfillment.VendorCollection, OrderState.Completed)]
    public void Next_returns_fulfilment_aware_state(OrderState current,Fulfillment fulfillment,OrderState expected)
    {
        Assert.Equal(expected,OrderWorkflow.Next(current,fulfillment));
        Assert.True(OrderWorkflow.CanTransition(current,expected,fulfillment));
    }

    [Theory]
    [InlineData(OrderState.Pending,true)]
    [InlineData(OrderState.Processing,true)]
    [InlineData(OrderState.Ready,false)]
    [InlineData(OrderState.Shipped,false)]
    [InlineData(OrderState.Completed,false)]
    public void Cancellation_is_limited_to_early_states(OrderState state,bool expected) =>
        Assert.Equal(expected,OrderWorkflow.CanCancel(state));

    [Fact]
    public void RestoreStock_returns_every_ordered_quantity()
    {
        var rice=new MenuItem{Stock=3};
        var drink=new MenuItem{Stock=8};
        var order=new FoodOrder{Lines=new List<OrderLine>{
            new(){MenuItem=rice,Quantity=2},
            new(){MenuItem=drink,Quantity=4}
        }};

        OrderWorkflow.RestoreStock(order);

        Assert.Equal(5,rice.Stock);
        Assert.Equal(12,drink.Stock);
    }

    [Fact]
    public void Delivery_cannot_skip_from_ready_to_completed() =>
        Assert.False(OrderWorkflow.CanTransition(OrderState.Ready,OrderState.Completed,Fulfillment.Delivery));
}

