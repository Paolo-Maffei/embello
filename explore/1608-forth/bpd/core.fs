\ core definitions

\ <<<board>>>
compiletoflash
( core start: ) here hex.

\ include ../flib/mecrisp/quotation.fs
\ include ../flib/mecrisp/multi.fs
\ include ../flib/any/timed.fs

\ PC13 constant LED

\ : led-init  omode-pp LED io-mode!  LED ios! ;
\ : led-on  LED ioc! ;
\ : led-off  LED ios! ;

cornerstone <<<core>>>
hello
