This is the code for a PDP8 on a Blue Pill. No frills. Nothing attached to it.  
See <https://docs.jeelabs.org/projects/pdp/#pdp-8-in-a-blue-pill>.

### Installation

* set up the Blue Pill with Mecrisp + USB, see the `../suf/` directory
* launch `folie`, see <https://github.com/jeelabs/folie>
* make sure the BP responds with an `ok.` prompt
* optional: `eraseflash`, then `!s board.fs`, then `!s core.fs`
* make sure there are no old definitions in flash: type `<<<core>>>`
* load the FOCAL image into flash: type `!s focal.fs`
* check available memory space: type `hello`
* load the PDP8 emulator into flash: type `compiletoflash`, then `!s pdp8.fs`
* that's it, the BP now contains FOCAL and the PDP-8 emulator
* to run it, type `go` and to quit, press the `reset` button on the BP

The `focal.fs` code was generated from `focal.rom` by `focal2fs.py`, so that  
the binary data can be loaded into flash (see constants `ROM` and `ROM-SIZE`).
