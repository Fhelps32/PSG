using Microsoft.AspNetCore.Mvc;
using PSG.Presentation.Models.Incricao;

namespace PSG.Presentation.Controllers
{
    public class InscricaoController : Controller
    {
        public IActionResult Index()
        {
            // Monta o ViewModel com a lista vazia por enquanto.
            // A view já fica com o loop pronto e exibe o estado vazio;
            // basta ligar o repositório/serviço aqui depois para popular Inscricoes.
            var model = new InscricaoIndexViewModel
            {
                Inscricoes = new List<InscricaoItemViewModel>()
            };

            return View(model);
        }
    }
}
