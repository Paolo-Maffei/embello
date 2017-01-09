\ application setup and main loop
\ assumes that the GPS is connected to usart2 on PA2&PA3

: TESTS ;
include ../flib/any/testing.fs

test-summary

0 constant debug  \ 0 = send RF packets, 1 = display on serial port
1 constant rate   \ seconds between readings

: s>uart2 ( c-addr len -- ) \ send string to uart2
  0 ?do dup i + c@ uart2-emit loop drop ;

: s>gps ( c-addr len -- ) \ send command string to GPS
  s" PMTK"  s>uart2 s>uart2 13 uart2-emit 10 uart2-emit
  ;

: gps? \ print GPS info
  s" 605,*31" s>gps
  ;

: fwd
  begin uart2-key? if uart2-key emit then key? until ." OK" ;

16MHz 1000 systick-hz
uart2-init
