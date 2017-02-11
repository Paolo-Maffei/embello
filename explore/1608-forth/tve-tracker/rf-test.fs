reset
cr

include ../flib/spi/lora1276.fs

." 4335  6 rf-init 1234 0 <# #s #> $A5 rf-send" cr
432592 $CB rf-init
\ rf!bw62cr6sf9
rf!bw10cr6sf7
: tx-cont begin 1234 0 <# #s #> $A5 rf-send [char] # emit 100 ms key? until ;
\ cr tx-cont

66 buffer: txbuf
0 variable txlen
: tx-start ( -- ) 0 txlen ! ;
: mem++ ( addr -- n ) dup @ dup 1+ rot ! ;
: tx+ ( b -- ) txlen mem++ txbuf + c! ;

: tx1 tx-start $81 tx+
  [char] H tx+ [char] i tx+
  txbuf txlen @
  2dup + rf+info
  2+ $2A rf-send ;

: txa
  tx1 rf-ack? ?dup if
    led-on 500 ms led-off
    ." LoRa ACK " rf>uart ." : " . cr
  else 500 ms ." Lost" cr then ;

: txa-loop led-off cr begin txa key? until ;
txa-loop
\ rf-listen
