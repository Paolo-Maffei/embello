# tve-central

The tve-central is a central node to display rf packet on an oled.
The board for it is composed of:
- hy-tiny (F103)
- oled, scl-PB6, sda-PB7
- rfm69cw, SPI on PA4-PA7

Software:
- mecrisp w/usb console from `suf/usb-hytiny.hex`
- `always.fs` is already in usb-mecrisp...
- `board.fs` (came from `tex/board.fs`)
- `main.fs` (came from `tex/e-rf.fs`)
