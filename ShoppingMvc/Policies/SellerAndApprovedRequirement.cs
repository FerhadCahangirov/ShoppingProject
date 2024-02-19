using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ShoppingMvc.Contexts;


public class SellerAndApprovedRequirement : IAuthorizationRequirement
{
}

public class SellerAndApprovedHandler : AuthorizationHandler<SellerAndApprovedRequirement>
{
    private readonly EvaraDbContext _db;

    public SellerAndApprovedHandler(EvaraDbContext db)
    {
        _db = db;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SellerAndApprovedRequirement requirement)
    {
        if (!context.User.Identity.IsAuthenticated || !context.User.IsInRole("Seller"))
        {
            context.Fail();
            return;
        }

        var userName = context.User.Identity.Name;

        var sellerData = await _db.SellerDatas
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(x => x.Seller.UserName == userName);

        if (sellerData != null && sellerData.IsApproved == true) 
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }

}
