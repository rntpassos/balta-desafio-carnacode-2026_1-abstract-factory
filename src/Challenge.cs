// DESAFIO: Sistema de Pagamentos Multi-Gateway
// PROBLEMA: Uma plataforma de e-commerce precisa integrar com múltiplos gateways de pagamento
// (PagSeguro, MercadoPago, Stripe) e cada gateway tem componentes específicos (Processador, Validador, Logger)
// O código atual está muito acoplado e dificulta a adição de novos gateways

// SOLUÇÃO: Refatorado usando o padrão Abstract Factory
//
// Estrutura do padrão neste código:
//   - Abstract Products:  IPaymentValidator, IPaymentProcessor, IPaymentLogger
//   - Concrete Products:  PagSeguro*, MercadoPago*, Stripe* (Validator/Processor/Logger)
//   - Abstract Factory:   IPaymentGatewayFactory
//   - Concrete Factories: PagSeguroFactory, MercadoPagoFactory, StripeFactory
//   - Client:             PaymentService

using System;

namespace DesignPatternChallenge
{
    public interface IPaymentValidator
    {
        bool ValidateCard(string cardNumber);
    }

    public interface IPaymentProcessor
    {
        string ProcessTransaction(decimal amount, string cardNumber);
    }

    public interface IPaymentLogger
    {
        void Log(string message);
    }

    public class PagSeguroValidator : IPaymentValidator
    {
        public bool ValidateCard(string cardNumber) 
        {
            Console.WriteLine("PagSeguro: Validando cartão...");
            return cardNumber.Length == 16;
        }
    }

    public class PagSeguroProcessor : IPaymentProcessor
    {
        public string ProcessTransaction(decimal amount, string cardNumber)
        {
            Console.WriteLine($"PagSeguro: Processando R$ {amount}...");
            return $"PAGSEG-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }

    public class PagSeguroLogger : IPaymentLogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[PagSeguro Log] {DateTime.Now}: {message}");
        }
    }

    public class MercadoPagoValidator : IPaymentValidator
    {
        public bool ValidateCard(string cardNumber)
        {
            Console.WriteLine("MercadoPago: Validando cartão...");
            return cardNumber.Length == 16 && cardNumber.StartsWith("5");
        }
    }

    public class MercadoPagoProcessor : IPaymentProcessor
    {
        public string ProcessTransaction(decimal amount, string cardNumber)
        {
            Console.WriteLine($"MercadoPago: Processando R$ {amount}...");
            return $"MP-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }

    public class MercadoPagoLogger : IPaymentLogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[MercadoPago Log] {DateTime.Now}: {message}");
        }
    }

    public class StripeValidator : IPaymentValidator
    {
        public bool ValidateCard(string cardNumber)
        {
            Console.WriteLine("Stripe: Validando cartão...");
            return cardNumber.Length == 16 && cardNumber.StartsWith("4");
        }
    }

    public class StripeProcessor : IPaymentProcessor
    {
        public string ProcessTransaction(decimal amount, string cardNumber)
        {
            Console.WriteLine($"Stripe: Processando ${amount}...");
            return $"STRIPE-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }

    public class StripeLogger : IPaymentLogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[Stripe Log] {DateTime.Now}: {message}");
        }
    }

    public interface IPaymentGatewayFactory
    {
        IPaymentValidator CreateValidator();
        IPaymentProcessor CreateProcessor();
        IPaymentLogger CreateLogger();
    }

    public class PagSeguroFactory : IPaymentGatewayFactory
    {
        public IPaymentValidator CreateValidator() => new PagSeguroValidator();
        public IPaymentProcessor CreateProcessor() => new PagSeguroProcessor();
        public IPaymentLogger CreateLogger() => new PagSeguroLogger();
    }

    public class MercadoPagoFactory : IPaymentGatewayFactory
    {
        public IPaymentValidator CreateValidator() => new MercadoPagoValidator();
        public IPaymentProcessor CreateProcessor() => new MercadoPagoProcessor();
        public IPaymentLogger CreateLogger() => new MercadoPagoLogger();
    }

    public class StripeFactory : IPaymentGatewayFactory
    {
        public IPaymentValidator CreateValidator() => new StripeValidator();
        public IPaymentProcessor CreateProcessor() => new StripeProcessor();
        public IPaymentLogger CreateLogger() => new StripeLogger();
    }

    public class PaymentService
    {
        private readonly IPaymentGatewayFactory _factory;

        public PaymentService(IPaymentGatewayFactory factory)
        {
            _factory = factory;
        }

        public void ProcessPayment(decimal amount, string cardNumber)
        {
            var validator = _factory.CreateValidator();
            if (!validator.ValidateCard(cardNumber))
            {
                Console.WriteLine("Cartão inválido");
                return;
            }

            var processor = _factory.CreateProcessor();
            var result = processor.ProcessTransaction(amount, cardNumber);

            var logger = _factory.CreateLogger();
            logger.Log($"Transação processada: {result}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Sistema de Pagamentos (Abstract Factory) ===\n");

            IPaymentGatewayFactory pagSeguroFactory = new PagSeguroFactory();
            var pagSeguroService = new PaymentService(pagSeguroFactory);
            pagSeguroService.ProcessPayment(150.00m, "1234567890123456");

            Console.WriteLine();

            IPaymentGatewayFactory mercadoPagoFactory = new MercadoPagoFactory();
            var mercadoPagoService = new PaymentService(mercadoPagoFactory);
            mercadoPagoService.ProcessPayment(200.00m, "5234567890123456");

            Console.WriteLine();

            IPaymentGatewayFactory stripeFactory = new StripeFactory();
            var stripeService = new PaymentService(stripeFactory);
            stripeService.ProcessPayment(300.00m, "4234567890123456");

            // Pergunta para reflexão:
            // - Como adicionar um novo gateway sem modificar PaymentService?
            // R: Basta criar novos Concrete Products (ex: CieloValidator, CieloProcessor, CieloLogger) e uma nova Concrete Factory (CieloFactory).
            // - Como garantir que todos os componentes de um gateway sejam compatíveis entre si?
            // R: Cada factory cria APENAS os componentes do seu gateway. PagSeguroFactory nunca vai retornar um StripeProcessor.
            // - Como evitar criar componentes de gateways diferentes acidentalmente?
            // R: O código cliente só interage com a factory (IPaymentGatewayFactory), nunca instancia os componentes diretamente. A factory garante a coesão.
        }
    }
}
