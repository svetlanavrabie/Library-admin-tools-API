using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Services;
using LibraryAPI.ActionConstrains;
using LibraryAPI.Dtos;
using LibraryAPI.Helpers;
using LibraryAPI.ResourcesParameters;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/authors")]

    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;
        public AuthorsController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper, IPropertyMappingService propertyMappingService,
                                 IPropertyCheckerService propertyCheckerService)
        {
            _courseLibraryRepository = courseLibraryRepository ?? throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        [HttpGet(Name = "GetAuthors")]
        [HttpHead]
        public IActionResult GetAuthors([FromQuery] AuthorResourcesParameters resourcesParameters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(resourcesParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(resourcesParameters.Fields))
            {
                return BadRequest();
            }

            var authors = _courseLibraryRepository.GetAuthors(resourcesParameters);

            var paginationMetadata = new
            {
                totalCount = authors.TotalCount,
                pageSize = authors.PageSize,
                currentPage = authors.CurrentPage,
                totalPages = authors.TotalPages
            };

            Response.Headers["X-Pagination"] = System.Text.Json.JsonSerializer.Serialize(paginationMetadata);

            var links = CreateLinksForAuthors(resourcesParameters, authors.HasNext, authors.HasPrevious);

            var shapedAuthors = _mapper.Map<IEnumerable<AuthorDto>>(authors).ShapeData(resourcesParameters.Fields);

            var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
            {
                var authorAsDictionary = author as IDictionary<string, object>;
                var authorLinks = CreateLinksForAuthor((Guid)authorAsDictionary["Id"], null);
                authorAsDictionary.Add("links", authorLinks);
                return authorAsDictionary;
            });

            var linkedCollectionResource = new
            {
                value = shapedAuthorsWithLinks,
                links
            };

            return Ok(linkedCollectionResource);
        }
        [Produces("application/json", "application/vdn.marvin.hateoas+json", "application/vdn.marvin.author.full+json",
            "application/vdn.marvin.author.full.hateoas+json", "application/vdn.marvin.author.friendly+json",
            "application/vdn.marvin.author.friendly.hateoas+json")]
        [HttpGet("{authorId}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid authorId, string fields, [FromHeader(Name = "Accept")] string mediaType)
        {

            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
            {
                return BadRequest();
            }

            var author = _courseLibraryRepository.GetAuthor(authorId);

            if (author == null)
            {
                return NotFound();
            }

            var includesLinks = parsedMediaType.SubTypeWithoutSuffix.EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);

            IEnumerable<LinkDto> links = new List<LinkDto>();

            if (includesLinks)
            {
                links = CreateLinksForAuthor(authorId, fields);
            }

            var primaryMediaType = includesLinks ? parsedMediaType.SubTypeWithoutSuffix
                       .Substring(0, parsedMediaType.SubTypeWithoutSuffix.Length - 8)
                       : parsedMediaType.SubTypeWithoutSuffix;

            if (primaryMediaType == "vdn.marvin.author.full")
            {
                var fullResourceToRetourn = _mapper.Map<AuthorFullDto>(author).ShapeData(fields)
                    as IDictionary<string, object>;

                if (includesLinks)
                {
                    fullResourceToRetourn.Add("links", links);
                }

                return Ok(fullResourceToRetourn);
            }

            var friendlyResourceToRetourn = _mapper.Map<AuthorDto>(author).ShapeData(fields)
                    as IDictionary<string, object>;

            if (includesLinks)
            {
                friendlyResourceToRetourn.Add("links", links);
            }

            return Ok(friendlyResourceToRetourn);
        }

        [HttpPost(Name = "CreateAuthorWithDateOfDeath")]
        [RequestGHeaderMatchesMediaTypeAttribute("Content-Type",
            "application/json", "application/vdn.marvin.authorforcreationwithdateofdeath+json")]
        [Consumes("application/vdn.marvin.authorforcreationwithdateofdeath+json")]
        public ActionResult<AuthorDto> CreateAuthorWithDateOfDeath(CreateAuthorWithDateOfDeathDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = _mapper.Map<Author>(author);

            _courseLibraryRepository.AddAuthor(authorEntity);

            _courseLibraryRepository.Save();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

            var links = CreateLinksForAuthor(authorToReturn.Id, null);

            var linkedResourceToReturn = authorToReturn.ShapeData(null)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetAuthor", new { authorId = linkedResourceToReturn["Id"] }, linkedResourceToReturn);
        }

        [HttpPost(Name = "CreateAuthor")]
        [RequestGHeaderMatchesMediaTypeAttribute("Content-Type",
            "application/json", "application/vdn.marvin.authorforcreation+json")]
        [Consumes("application/json", "application/vdn.marvin.authorforcreation+json")]
        public ActionResult<AuthorDto> CreateAuthor(CreateAuthorDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = _mapper.Map<Author>(author);

            _courseLibraryRepository.AddAuthor(authorEntity);

            _courseLibraryRepository.Save();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

            var links = CreateLinksForAuthor(authorToReturn.Id, null);

            var linkedResourceToReturn = authorToReturn.ShapeData(null)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetAuthor", new { authorId = linkedResourceToReturn["Id"] }, linkedResourceToReturn);
        }



        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {

            Response.Headers["Allow"] = "GET,OPTIONS,POST";

            return Ok();
        }

        [HttpDelete("{authorId}", Name = "DeleteAuthor")]
        public ActionResult DeleteAuthor(Guid authorId)
        {
            var author = _courseLibraryRepository.GetAuthor(authorId);

            if (author == null)
            {
                return NotFound();
            }

            _courseLibraryRepository.DeleteAuthor(author);

            _courseLibraryRepository.Save();

            return NoContent();
        }

        private string CreateAuthorsResourceUri(AuthorResourcesParameters authorResourcesParameters,
                                                 ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetAuthors",
                        new
                        {
                            fields = authorResourcesParameters.Fields,
                            orderBy = authorResourcesParameters.OrderBy,
                            pageNumber = authorResourcesParameters.PageNumber - 1,
                            pageSize = authorResourcesParameters.PageSize,
                            mainCategory = authorResourcesParameters.MainCategory,
                            searchQuery = authorResourcesParameters.Search
                        });

                case ResourceUriType.NextPage:
                    return Url.Link("GetAuthors",
                        new
                        {
                            fields = authorResourcesParameters.Fields,
                            orderBy = authorResourcesParameters.OrderBy,
                            pageNumber = authorResourcesParameters.PageNumber + 1,
                            pageSize = authorResourcesParameters.PageSize,
                            mainCategory = authorResourcesParameters.MainCategory,
                            searchQuery = authorResourcesParameters.Search
                        });
                case ResourceUriType.Current:
                default:
                    return Url.Link("GetAuthors",
                         new
                         {
                             fields = authorResourcesParameters.Fields,
                             orderBy = authorResourcesParameters.OrderBy,
                             pageNumber = authorResourcesParameters.PageNumber,
                             pageSize = authorResourcesParameters.PageSize,
                             mainCategory = authorResourcesParameters.MainCategory,
                             searchQuery = authorResourcesParameters.Search
                         });
            }
        }

        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid authorId, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(Url.Link("GetAuthor", new { authorId }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(Url.Link("GetAuthor", new { authorId, fields }),
                  "self",
                  "GET"));
            }

            links.Add(
               new LinkDto(Url.Link("DeleteAuthor", new { authorId }),
               "delete_author",
               "DELETE"));

            links.Add(
                new LinkDto(Url.Link("CreateCourseForAthor", new { authorId }),
                "create_course_for_author",
                "POST"));

            links.Add(
               new LinkDto(Url.Link("GetCoursesForAuhtor", new { authorId }),
               "courses",
               "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors(AuthorResourcesParameters authorsResourcesParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateAuthorsResourceUri(authorsResourcesParameters, ResourceUriType.Current),
               "self",
               "GET"));

            if (hasNext)
            {
                links.Add(
                new LinkDto(CreateAuthorsResourceUri(authorsResourcesParameters, ResourceUriType.NextPage),
                "nextPage",
                "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                new LinkDto(CreateAuthorsResourceUri(authorsResourcesParameters, ResourceUriType.PreviousPage),
                "previousPage",
                "GET"));
            }

            return links;
        }
    }
}

