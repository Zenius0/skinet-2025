using System;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class CouponsController(ICouponService couponService) : BaseApiController
{
    [HttpGet("{code}")]
    public async Task<ActionResult<AppCoupon>> ValidateCoupon(string code)
    {
        try
        {
            var coupon = await couponService.GetCouponFromPromoCode(code);

            if (coupon == null) return BadRequest("Invalid voucher code");

            return coupon;
        }
        catch (Exception ex)
        {
            return BadRequest($"Error validating coupon: {ex.Message}");
        }
    }
}
