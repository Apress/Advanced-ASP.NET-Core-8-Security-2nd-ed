$(".product-quantity").change(function () {
    var productID = $(this).attr("product");
    var quantity = $(this).val();
    var price = $(this).attr("price");

    var totalPrice = Number(quantity) * Number(price);
    $("#total-" + productID).text("$" + totalPrice.toFixed(2));

    var total = 0;

    $(".product-quantity").each(function () {
        var quantity = $(this).val();
        var price = $(this).attr("price");

        total += Number(quantity) * Number(price);
    });

    $("#Total").text(total.toFixed(2));
});