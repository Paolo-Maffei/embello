This is the code for a PDP8 on a Blue Pill. No frills. Nothing attached to it.

### Installation

* set up the Blue Pill with Mecrisp + USB, see the `../suf/` directory
* launch `folie`, see <https://github.com/jeelabs/folie>
* make sure the BP responds with an `ok.` prompt
* optional: `eraseflash`, then `!s board.fs`, then `!s core.fs`
* make sure there are no old definitions in flash: type `<<<core>>>`
* load the DF32 disk image into flash: type `!s df32.fs`
* check available memory space: type `hello`
* load the PDP8 emulator into flash: type `compiletoflash`, then `!s pdp8.fs`
* that's it, the BP now contains 4K DMS on a virtual DF32 with PDP-8 emulator
* to run it, type `go` and to quit, press the `reset` button on the BP

The `df32.fs` virtual disk data was generated from `df32.mif` by `mif2fs.c`.
The command for this is: `gcc -o mif2fs mif2fs.c && ./mif2fs >df32.fs`.
