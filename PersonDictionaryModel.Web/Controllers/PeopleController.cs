﻿using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PersonDictionaryModel.Application.Interfaces;
using PersonDictionaryModel.Core.Application.Interfaces;
using PersonDictionaryModel.Core.Application.Utils;
using PersonDictionaryModel.Core.Domain.Models;
using PersonDictionaryModel.Core.Model.Models;
using PersonDictionaryModel.Core.Model.Resourcearameters;
using PersonDictionaryModel.Web.Filters;
using System;
using System.IO;
using System.Threading.Tasks;
using static PersonDictionaryModel.Core.Model.Constants.InMemoryCacheConstants;

namespace PersonDictionaryModel.Web.Controllers
{
    public class PeopleController : BaseController
    {
        private readonly IPersonRepository _personRepo;
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public PeopleController(
            IPersonRepository personRepo,
            IMapper mapper,
            IFileService fileService,
            IWebHostEnvironment env,
            IMemoryCache memoryCache) : base(memoryCache)
        {
            _personRepo = personRepo;
            _fileService = fileService;
            _mapper = mapper;
            _env = env;
        }

        #region GET
        [HttpGet("{personId}")]
        public async Task<IActionResult> GetById(int personId)
        {
            if (!_memoryCache.TryGetValue($"{PERSON_CACHE_KEY}:{personId}", out PersonDto person))
            {
                person = await _personRepo.GetByIdAsync(personId);
                _memoryCache.Set($"{PERSON_CACHE_KEY}:{personId}", person, InMemoryCache.MemoryCacheEntryOptions);
            }

            return Ok(person);
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] ResourceParameter resourceParameter)
        {
            var personListResponse = await _personRepo.SearchAsync(resourceParameter);
            return Ok(personListResponse);
        }
        #endregion

        #region POST
        [HttpPost]
        [ServiceFilter(typeof(ValidatePersonModelAttribute))]
        public async Task<IActionResult> CreatePerson([FromBody] CreatePersonDto createPersonDto)
        {
            string fileName = null;
            string extension = null;

            if (!(createPersonDto.Photo is null))
            {
                var filePath = _env.ContentRootPath;
                var content = await _fileService.GetBytesAsync(createPersonDto.Photo);

                fileName = Guid.NewGuid().ToString();
                extension = Path.GetExtension(createPersonDto.Photo.FileName);

                await _fileService.SaveFileAsync(content, filePath, fileName, extension);
            }

            var personToAdd = _mapper.Map<Person>(createPersonDto);

            personToAdd.Photo = fileName;
            personToAdd.Extension = extension;

            var addedPerson = await _personRepo.AddAsync(personToAdd);

            _memoryCache.Set($"{PERSON_CACHE_KEY}:{addedPerson.Id}", addedPerson);

            return Ok(addedPerson);
        }
        #endregion

        #region PUT
        [HttpPut]
        [ServiceFilter(typeof(ValidatePersonModelAttribute))]
        public async Task<IActionResult> UpdatePerson([FromBody] UpdatePersonDto updatePersonDto)
        {
            _memoryCache.Remove($"{PERSON_CACHE_KEY}:{updatePersonDto.Id}");
            var personToUpdate = _mapper.Map<Person>(updatePersonDto);
            var updatedPerson = await _personRepo.UpdateAsync(personToUpdate);

            _memoryCache.Set($"{PERSON_CACHE_KEY}:{updatePersonDto.Id}", updatedPerson);

            return NoContent();
        }
        #endregion

        #region DELETE
        [HttpDelete("{personId}")]
        public async Task<IActionResult> DeletePerson(int personId)
        {
            await _personRepo.DeleteAsync(personId);

            _memoryCache.Remove($"{PERSON_CACHE_KEY}:{personId}");

            return NoContent();
        }
        #endregion

    }
}
