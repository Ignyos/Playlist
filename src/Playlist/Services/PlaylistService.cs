using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Models;

namespace Playlist.Services;

public class PlaylistService
{
    private readonly PlaylistDbContext _context;

    public PlaylistService(PlaylistDbContext context)
    {
        _context = context;
    }

    public List<Models.Playlist> GetAllPlaylists(string? searchTerm = null)
    {
        var query = _context.Playlists
            .Where(p => p.DeleteDate == null);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm));
        }

        return query.OrderBy(p => p.Name).ToList();
    }

    public Models.Playlist? GetPlaylistById(int id)
    {
        return _context.Playlists
            .Include(p => p.Items.Where(i => i.DeleteDate == null))
            .FirstOrDefault(p => p.Id == id && p.DeleteDate == null);
    }

    public Models.Playlist CreatePlaylist(string name, List<string> filePaths)
    {
        var playlist = new Models.Playlist
        {
            Name = name,
            Created = DateTime.Now,
            LastPlayed = DateTime.MinValue
        };

        var ordinal = 0;
        foreach (var filePath in filePaths)
        {
            var fileName = System.IO.Path.GetFileName(filePath);
            playlist.Items.Add(new PlaylistItem
            {
                Path = filePath,
                Name = fileName,
                Ordinal = ordinal++,
                LastPlayed = DateTime.MinValue
            });
        }

        _context.Playlists.Add(playlist);
        _context.SaveChanges();

        return playlist;
    }

    public void UpdatePlaylist(int id, string name)
    {
        var playlist = _context.Playlists.Find(id);
        if (playlist != null && playlist.DeleteDate == null)
        {
            playlist.Name = name;
            _context.SaveChanges();
        }
    }

    public void DeletePlaylist(int id)
    {
        var playlist = _context.Playlists.Find(id);
        if (playlist != null)
        {
            playlist.DeleteDate = DateTime.Now;
            _context.SaveChanges();
        }
    }

    public List<PlaylistItem> GetPlaylistItems(int playlistId)
    {
        return _context.PlaylistItems
            .Where(i => i.PlaylistId == playlistId && i.DeleteDate == null)
            .OrderBy(i => i.Ordinal)
            .ToList();
    }

    public void AddItemsToPlaylist(int playlistId, List<string> filePaths)
    {
        var playlist = _context.Playlists
            .Include(p => p.Items)
            .FirstOrDefault(p => p.Id == playlistId);

        if (playlist == null) return;

        var maxOrdinal = playlist.Items
            .Where(i => i.DeleteDate == null)
            .Select(i => (int?)i.Ordinal)
            .Max() ?? -1;

        foreach (var filePath in filePaths)
        {
            var fileName = System.IO.Path.GetFileName(filePath);
            playlist.Items.Add(new PlaylistItem
            {
                Path = filePath,
                Name = fileName,
                Ordinal = ++maxOrdinal,
                LastPlayed = DateTime.MinValue
            });
        }

        _context.SaveChanges();
    }

    public void RemovePlaylistItem(int itemId)
    {
        var item = _context.PlaylistItems.Find(itemId);
        if (item != null)
        {
            item.DeleteDate = DateTime.Now;
            _context.SaveChanges();
        }
    }

    public void UpdatePlaylistItemName(int itemId, string newName)
    {
        var item = _context.PlaylistItems.Find(itemId);
        if (item != null && item.DeleteDate == null)
        {
            item.Name = newName;
            _context.SaveChanges();
        }
    }

    public void ReorderPlaylistItems(int playlistId, List<int> itemIdsInOrder)
    {
        var items = _context.PlaylistItems
            .Where(i => i.PlaylistId == playlistId && i.DeleteDate == null)
            .ToList();

        for (int i = 0; i < itemIdsInOrder.Count; i++)
        {
            var item = items.FirstOrDefault(x => x.Id == itemIdsInOrder[i]);
            if (item != null)
            {
                item.Ordinal = i;
            }
        }

        _context.SaveChanges();
    }

    public void LogHistory(int playlistId, int playlistItemId)
    {
        _context.History.Add(new History
        {
            PlaylistId = playlistId,
            PlaylistItemId = playlistItemId,
            TimeStamp = DateTime.Now
        });

        var playlist = _context.Playlists.Find(playlistId);
        if (playlist != null)
        {
            playlist.LastPlayed = DateTime.Now;
        }

        var item = _context.PlaylistItems.Find(playlistItemId);
        if (item != null)
        {
            item.LastPlayed = DateTime.Now;
        }

        _context.SaveChanges();
    }

    public void UpdatePlaylistItemTimestamp(int itemId, int timestamp)
    {
        var item = _context.PlaylistItems.Find(itemId);
        if (item != null)
        {
            item.TimeStamp = timestamp;
            _context.SaveChanges();
        }
    }

    public void UpdatePlaylistSelectedItem(int playlistId, int? selectedItemId)
    {
        var playlist = _context.Playlists.Find(playlistId);
        if (playlist != null)
        {
            playlist.SelectedItemId = selectedItemId;
            _context.SaveChanges();
        }
    }
}
