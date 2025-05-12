using LHBooksWeb.Models.Vnpay;

namespace LHBooksWeb.Services.Vnpay
{
    public interface IVnPayService
    {
        
            string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
            VnPaymentResponseModel PaymentExecute(IQueryCollection collections);

        
    }
}
