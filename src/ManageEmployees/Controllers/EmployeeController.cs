﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManageEmployees.Data.Abstract;
using ManageEmployees.Data.Repositories;
using ManageEmployees.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ManageEmployees.Controllers
{
    [Route("api/[controller]")]
    public class EmployeeController : Controller
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IContractRepository _contractRepository;

        public EmployeeController(IDepartmentRepository departmentRepository, IEmployeeRepository employeeRepository, IContractRepository contractRepository)
        {
            this._departmentRepository = departmentRepository;
            this._employeeRepository = employeeRepository;
            this._contractRepository = contractRepository;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var employees = _employeeRepository.AllIncluding(p=>p.Department,c=>c.Contracts);

            if (employees != null) return Ok(employees);
            
            return NotFound();
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var employee = _employeeRepository.GetSingle(p=>p.Id == id,dep=>dep.Department,con=>con.Contracts);
            if (employee != null) return Ok(employee);

            return NotFound();
        }

        [HttpGet("{id}/contracts")]
        public IActionResult GetEmployeeContracts(int id)
        {
            var employeeContracts = _contractRepository.GetAll().Where(p => p.EmployeeId == id);

            if (employeeContracts != null)
                return Ok(employeeContracts);

            return NotFound();
        }
        
        [HttpPost]
        public IActionResult Post([FromBody] Employee employee)
        {
            try
            {
                var _employee = employee;
                if (_employee == null) throw new ArgumentNullException(nameof(_employee));
               
                _employeeRepository.Add(_employee);
                _employeeRepository.Commit();

                return Created($"/api/employee/{employee.Id}", _employee);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Employee employee)
        {
            try
            {
                var _employee = _employeeRepository.GetSingle(id);

                if (_employee == null) throw new ArgumentNullException(nameof(_employee));

                _employee.FirstName = employee.FirstName;
                _employee.LastName = employee.LastName;
                _employee.Age = employee.Age;
                _employee.BirthDate = employee.BirthDate;
                _employee.JobPosition = employee.JobPosition;
                
                if (_employeeRepository.GetSingle(employee.DepartmentId) == null) throw new ArgumentNullException($"No departments exist with ID you have selected.");
                _employee.DepartmentId = employee.DepartmentId;

                _employeeRepository.Commit();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Employee employee = _employeeRepository.GetSingle(id);

            if (employee == null) return new NotFoundResult();

            var employeeContracts = _contractRepository.FindBy(a => a.EmployeeId == id);

            foreach (var contract in employeeContracts)
                _contractRepository.Delete(contract);

            _employeeRepository.Delete(employee);
            _employeeRepository.Commit();

            return new NoContentResult();
        }
    }
}