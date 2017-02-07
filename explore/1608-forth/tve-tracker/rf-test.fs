reset
cr

include ../flib/spi/lora1276.fs

." 4335  6 rf-init 1234 0 <# #s #> $A5 rf-send" cr
4335  $CB rf-init
: tx-cont begin 1234 0 <# #s #> $A5 rf-send [char] # emit 100 ms key? until ;
\ cr tx-cont

66 buffer: txbuf
0 variable txlen
: tx-start ( -- ) 0 txlen ! ;
: mem++ ( addr -- n ) dup @ dup 1+ rot ! ;
: tx+ ( b -- ) txlen mem++ txbuf + c! ;
: tx1 tx-start 1 tx+ [char] H tx+ [char] i tx+  txbuf txlen @ $2A rf-send ;
: txa tx1 rf-ack? drop ;


\ rf-listen
