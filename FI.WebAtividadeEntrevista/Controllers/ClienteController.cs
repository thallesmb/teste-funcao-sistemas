﻿using FI.AtividadeEntrevista.BLL;
using FI.AtividadeEntrevista.DML;
using FI.WebAtividadeEntrevista.Models;
using FI.WebAtividadeEntrevista.Models.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebAtividadeEntrevista.Models;

namespace WebAtividadeEntrevista.Controllers
{
    public class ClienteController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Incluir()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Incluir(ClienteModel model)
        {
            BoCliente boCliente = new BoCliente();
            BoBeneficiario boBeneficiario = new BoBeneficiario();

            List<string> erros = new List<string>();

            if (!this.ModelState.IsValid)
            {
                erros = (from item in ModelState.Values
                         from error in item.Errors
                         select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else
            {
                if (CPFValidation.IsValid(model.CPF) && !CPFValidation.AlreadyExists(model.CPF, model.Id))
                {
                    model.Id = boCliente.Incluir(new Cliente()
                    {
                        CEP = model.CEP,
                        Cidade = model.Cidade,
                        Email = model.Email,
                        Estado = model.Estado,
                        Logradouro = model.Logradouro,
                        Nacionalidade = model.Nacionalidade,
                        Nome = model.Nome,
                        Sobrenome = model.Sobrenome,
                        Telefone = model.Telefone,
                        CPF = model.CPF
                    });

                    if (model.Beneficiarios != null)
                    {
                        foreach (BeneficiarioModel beneficiario in model.Beneficiarios)
                        {
                            if (CPFValidation.IsValid(beneficiario.CPF))
                            {
                                boBeneficiario.Incluir(new Beneficiario()
                                {
                                    CPF = beneficiario.CPF,
                                    Nome = beneficiario.Nome,
                                    ClienteId = model.Id
                                });
                            }
                            else
                                erros.Add($"O CPF: {beneficiario.CPF}, do cliente {beneficiario.Nome} é inválido.");
                        }

                        if (erros.Count > 0)
                        {
                            erros.Add("Cadastro parcialmente concluido.");
                            return Json(string.Join(Environment.NewLine, erros));
                        }
                    }

                    return Json("Cadastro concluido com sucesso");
                }
                else
                {
                    erros.Add($"O CPF: {model.CPF}, do cliente {model.Nome} é inválido ou já esta sendo utilizado no sistema.");
                }
            }

            Response.StatusCode = 400;
            return Json(string.Join(Environment.NewLine, erros));
        }

        [HttpPost]
        public JsonResult Alterar(ClienteModel model)
        {
            BoCliente boCliente = new BoCliente();
            BoBeneficiario boBeneficiario = new BoBeneficiario();

            List<string> erros = new List<string>();

            if (!this.ModelState.IsValid)
            {
                erros = (from item in ModelState.Values
                         from error in item.Errors
                         select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else
            {
                if (CPFValidation.IsValid(model.CPF) && !CPFValidation.AlreadyExists(model.CPF, model.Id))
                {
                    boCliente.Alterar(new Cliente()
                    {
                        Id = model.Id,
                        CEP = model.CEP,
                        Cidade = model.Cidade,
                        Email = model.Email,
                        Estado = model.Estado,
                        Logradouro = model.Logradouro,
                        Nacionalidade = model.Nacionalidade,
                        Nome = model.Nome,
                        Sobrenome = model.Sobrenome,
                        Telefone = model.Telefone,
                        CPF = model.CPF
                    });

                    if (model.Beneficiarios.Count > 0)
                    {
                        foreach (BeneficiarioModel beneficiario in model.Beneficiarios)
                        {
                            if (beneficiario.Acao == "Remove") // Remove sem precisar verificar CPF, pois poderia ocorrer um update (alterando cpf) e logo em seguida um remove via modal.
                            {
                                boBeneficiario.Excluir(beneficiario.Id);
                            }
                            else if (CPFValidation.IsValid(beneficiario.CPF)) // Valida o CPF antes de inserir ou alterar cada beneficiario (CPF duplicado já é validado no JS).
                            {
                                if (beneficiario.Acao == "Register")
                                {
                                    boBeneficiario.Incluir(new Beneficiario()
                                    {
                                        CPF = beneficiario.CPF,
                                        Nome = beneficiario.Nome,
                                        ClienteId = model.Id
                                    });
                                }
                                else if (beneficiario.Acao == "Update")
                                {
                                    boBeneficiario.Alterar(new Beneficiario()
                                    {
                                        Id = beneficiario.Id,
                                        CPF = beneficiario.CPF,
                                        Nome = beneficiario.Nome,
                                        ClienteId = model.Id
                                    });
                                }
                            }
                            else
                                erros.Add($"CPF {beneficiario.CPF} do cliente {beneficiario.Nome} é inválido.");
                        }

                        if (erros.Count > 0)
                        {
                            erros.Add("Cadastro parcialmente alterado.");
                            return Json(string.Join(Environment.NewLine, erros));
                        }
                    }

                    return Json("O cadastro foi alterado com sucesso.");
                }
                else
                    erros.Add($"O CPF: {model.CPF}, do cliente {model.Nome} é inválido.");
            }

            Response.StatusCode = 400;
            return Json(string.Join(Environment.NewLine, erros));
        }

        [HttpGet]
        public ActionResult Alterar(long id)
        {
            BoCliente bo = new BoCliente();
            Cliente cliente = bo.Consultar(id);
            Models.ClienteModel model = null;
            List<BeneficiarioModel> beneficiarios = new List<BeneficiarioModel>();

            if (cliente.Beneficiarios.Count > 0)
            {
                foreach (Beneficiario beneficiario in cliente.Beneficiarios)
                {
                    beneficiarios.Add(new BeneficiarioModel
                    {
                        Id = beneficiario.Id,
                        ClienteId = beneficiario.ClienteId,
                        CPF = beneficiario.CPF,
                        Nome = beneficiario.Nome
                    });
                }
            }

            if (cliente != null)
            {
                model = new ClienteModel()
                {
                    Id = cliente.Id,
                    CEP = cliente.CEP,
                    Cidade = cliente.Cidade,
                    Email = cliente.Email,
                    Estado = cliente.Estado,
                    Logradouro = cliente.Logradouro,
                    Nacionalidade = cliente.Nacionalidade,
                    Nome = cliente.Nome,
                    Sobrenome = cliente.Sobrenome,
                    Telefone = cliente.Telefone,
                    CPF = cliente.CPF,
                    Beneficiarios = beneficiarios
                };
            }

            return View(model);
        }

        [HttpPost]
        public JsonResult ClienteList(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                int qtd = 0;
                string campo = string.Empty;
                string crescente = string.Empty;
                string[] array = jtSorting.Split(' ');

                if (array.Length > 0)
                    campo = array[0];

                if (array.Length > 1)
                    crescente = array[1];

                List<Cliente> clientes = new BoCliente().Pesquisa(jtStartIndex, jtPageSize, campo, crescente.Equals("ASC", StringComparison.InvariantCultureIgnoreCase), out qtd);

                //Return result to jTable
                return Json(new { Result = "OK", Records = clientes, TotalRecordCount = qtd });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }
    }
}