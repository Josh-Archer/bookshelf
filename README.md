# bookshelf

This is a revival of [Readarr](https://github.com/Readarr/Readarr). The images
published are configured to use working Goodreads or Hardcover metadata out of
the box.

Bookshelf is an ebook and audiobook collection manager for Usenet and BitTorrent
users. It can monitor multiple RSS feeds for new books from your favorite
authors and will grab, sort, and rename them. Note that only one type of a
given book is supported. If you want both an audiobook and ebook of a given
book you will need multiple instances.

## Getting Started

The container listens on port 8787 and expects a volume mounted at `/config`.

    docker run -p 8787:8787 -v ~/.config/bookshelf:/config ghcr.io/pennydreadful/bookshelf:hardcover

The `softcover` tags use [Goodreads](https://www.goodreads.com) as the metadata
provider. The quality of this metadata is generally poor and contains a lot of
slop. However, it is backward-compatible with existing Readarr databases and
functionality like Goodreads list imports should continue to work normally.

The `hardcover` tags use [Hardcover](https://hardcover.app/home) as a metadata
provider. This metadata is higher quality but isn't backward-compatible; if
you're already running Readarr you'll need to redeploy this from scratch.
Goodreads list imports haven't been tested and likely don't work.

## Multi-User CWA Routing

When using a single `bookshelf` instance to feed Calibre-Web-Automated for more
than one user:

- Create a separate Hardcover import-list definition per user or per destination
  route.
- Set each import list's `RootFolderPath` to a distinct ingest path such as
  `/cwa-book-ingest/erin` or `/cwa-book-ingest/mine`.
- The last path segment is just a routing key for CWA. It can be the exact CWA
  username (for example `erin@archerfamily.io`) or a shorter alias that CWA
  resolves via its `User-Routed Ingest` mapping.
- In Calibre-Web-Automated, map those folder keys to actual CWA users with the
  `User-Routed Ingest` setting when the folder name is not already the username.

Current limitation:

- Items are flattened and deduplicated by author/title across enabled import
  lists before import processing continues.
- If the same book appears on more than one user's Hardcover list, only one
  import-list definition survives for that book, so downstream per-user routing
  is not guaranteed for shared titles.

## Support

This project won't use Discord for support. If you have a problem please file
an issue or start a discussion.

## Contributors & Developers

Help is very welcome. Priority is on fixing quality of life issues

- [ ] Monitor series.
- [ ] Support ebook and audio files in the same root.

Already done

- [x] Native support for MyAnonaMouse without Prowlarr.
- [x] Hardcover list import.
- [x] Improved matching.
- [x] Metadata is no longer cached locally.
- [x] Removed servarr analytics spyware.
- [x] Supports selfhosted metadata (UI or `METADATA_URL` env var).

## Sponsors

If you ever donated to [this](https://opencollective.com/readarr) project you
should request a refund. Those people don't deserve your money.

### License

The is a derivative work of the [Readarr](https://github.com/Readarr/Readarr)
and [Prowlarr](https://github.com/Prowlarr/Prowlarr) projects which are both
licensed [GPLv3](http://www.gnu.org/licenses/gpl.html). This project is
therefore also licensed under the terms of GPLv3.

Copyright 2025-2026
