---
title: "Assets - Video"
date: 2017-10-17T15:26:15Z
draft: false
weight: 40
---

Notes and reminders on some assets and modes.

## Videos

### Video format
---

.Webm is buggy windows? and have had better results with `.ogv`.

### FFMPEG: Converting video examples
---

Using `FFMPEG` when converting from mp4 *this file will be twice the size of the mp4 (6)*

`ffmpeg -i input.mp4 -c:v libtheora -qscale:v 6 -c:a libvorbis -qscale:a 6 outputname.ogv`

Take 10 seconds of video starting at 12 seconds * This lower quality (3) will produce about the same size as the mp4*

`ffmpeg -ss 12 -t 10 -i input.mp4 -c:v libtheora -qscale:v 3 -c:a libvorbis -qscale:a 3 outputname.ogv`

- Join with other audio

`ffmpeg -i "video.avi" -i "audio.ogg" -map 0:v -map 1:a -c:v copy -codec:v libtheora -qscale:v 6 -codec:a libvorbis -qscale:a 6 output.ogv`

- Reverse

`ffmpeg -i inputvideo.ogv -vf reverse outputvideo_reversed.ogv`

- Join

`ffmpeg -i "concat:in.ogv|in2.ogv" -codec copy output.ogv`

- Create image frames

`ffmpeg -i Whirl.mp4 "%03d.png"`

- Image to OGV

`ffmpeg -framerate 25 -i image_%d.png -qscale:v 6 test2.ogv`


