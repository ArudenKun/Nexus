using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using MiniExcelLibs;
using Nexus.Permissions;
using Nexus.Shared;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Caching;
using Volo.Abp.Content;

namespace Nexus.Authors;

[Authorize(NexusPermissions.Authors.Default)]
public class AuthorAppService : NexusAppService<IAuthorRepository>, IAuthorAppService
{
    private readonly IDistributedCache<
        AuthorExcelDownloadTokenCacheItem,
        string
    > _excelDownloadTokenCache;

    public AuthorAppService(
        IDistributedCache<AuthorExcelDownloadTokenCacheItem, string> excelDownloadTokenCache
    )
    {
        _excelDownloadTokenCache = excelDownloadTokenCache;
    }

    public async Task<AuthorDto> GetAsync(Guid id)
    {
        var author = await Repository.GetAsync(id);
        return ObjectMapper.Map<Author, AuthorDto>(author);
    }

    public async Task<PagedResultDto<AuthorDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await Repository.GetQueryableAsync();
        var query = queryable
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "Name" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var authors = await AsyncExecuter.ToListAsync(query);
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        return new PagedResultDto<AuthorDto>(
            totalCount,
            ObjectMapper.Map<List<Author>, List<AuthorDto>>(authors)
        );
    }

    [Authorize(NexusPermissions.Authors.Create)]
    public async Task<AuthorDto> CreateAsync(CreateUpdateAuthorDto input)
    {
        var existingAuthor = await Repository.FindByNameAsync(input.Name);
        if (existingAuthor is not null)
        {
            throw new UserFriendlyException($"Author {input.Name} already exists");
        }

        var author = ObjectMapper.Map<CreateUpdateAuthorDto, Author>(input);
        await Repository.InsertAsync(author);
        return ObjectMapper.Map<Author, AuthorDto>(author);
    }

    [Authorize(NexusPermissions.Authors.Edit)]
    public async Task<AuthorDto> UpdateAsync(Guid id, CreateUpdateAuthorDto input)
    {
        var author = await Repository.GetAsync(id);
        ObjectMapper.Map(input, author);
        await Repository.UpdateAsync(author);
        return ObjectMapper.Map<Author, AuthorDto>(author);
    }

    [Authorize(NexusPermissions.Authors.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await Repository.DeleteAsync(id);
    }

    [AllowAnonymous]
    public async Task<IRemoteStreamContent> GetListAsExcelFileAsync(AuthorExcelDownloadDto input)
    {
        var downloadToken = await _excelDownloadTokenCache.GetAsync(input.DownloadToken);
        if (downloadToken == null || input.DownloadToken != downloadToken.Token)
        {
            throw new AbpAuthorizationException("Invalid download token: " + input.DownloadToken);
        }

        var queryable = await Repository.GetQueryableAsync();
        var query = queryable.OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "Name" : input.Sorting);
        var authors = await AsyncExecuter.ToListAsync(query);

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(
            ObjectMapper.Map<List<Author>, List<AuthorExcelDto>>(authors)
        );
        memoryStream.Seek(0, SeekOrigin.Begin);

        return new RemoteStreamContent(
            memoryStream,
            "Authors.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }

    public async Task<DownloadTokenResultDto> GetDownloadTokenAsync()
    {
        var token = GuidGenerator.Create().ToString("N");

        await _excelDownloadTokenCache.SetAsync(
            token,
            new AuthorExcelDownloadTokenCacheItem { Token = token },
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
            }
        );

        return new DownloadTokenResultDto { Token = token };
    }
}
