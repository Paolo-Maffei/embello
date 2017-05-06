\ tests for hc05.fs

: .v ( ... -- ... )  \ view stack, this is a slightly cleaner version of .s
  depth 100 u< if
    ." Stack #" depth . ." < "
    -1 depth negate ?do
      sp@ i 2+ cells - @
      dup $10000 u> if [char] $ emit hex. else . then
    loop
    ." >" cr
  else
    ." Stack underflow (" depth . ." )" cr
  then ;

: empty-stack ( any -- ) \ checks that the stack is empty, if not flash LED and empty it
  depth if
    ." *** " depth . ." left on stack: " .v
    depth 100 u< if
      depth 0 do drop loop
    then
  then ;

include ../flib/any/testing.fs

\ for tests we read input from a pre-filled buffer instead of reading from uart
0 variable inptr \ points to next char to consume
: uart2-key inptr dup @ dup c@ swap 1+ rot ! ; \ return next char at inbuf

include hc05.fs

200 buffer: inbuf

: add-crlf ( c-addr len -- c-addr len+2 ) \ append CR-LF to string
  2dup + RET swap c! 1+
  2dup + LF swap c! 1+
  ;

: buffer-cpy ( dest-addr src-addr cnt -- dest-addr cnt )
  swap 2 pick 2 pick ( dst cnt src dst cnt )
  move
  ;

: set-input ( c-addr len -- )
  inbuf -rot buffer-cpy
  add-crlf
  drop inptr !
  ;

: string-eq ( c-addr1 len1 c-addr2 len2 -- f )
  2 pick = if ( c-addr1 len1 c-addr2 )
    swap 0 ?do ( c-addr1 c-addr2 )
      dup i + c@ ( c-addr1 c-addr2 c2 )
      2 pick i + c@ ( c-addr1 c-addr2 c2 c1 )
      <> if 2drop 0 unloop exit then
    loop
    2drop -1
  else
    2drop drop 0 \ different lengths
  then ;
: s s" hello world" ; s s string-eq -1 =always
: r s" hello worlD" ; s r string-eq 0 =always
: r s" hello" ; s r string-eq 0 =always

16MHz 1000 systick-hz
uart2-init
38400 uart2-baud

\ test read-line and not-OK
s set-input read-line 11 =always
s set-input read-line inbuf swap s string-eq -1 =always
s set-input read-line not-OK? 11 =always
: k s" OK" ; k set-input read-line 2 =always
k set-input read-line not-OK? 0 =always
empty-stack

\ test get-response
k set-input get-response 0 =always drop
s tuck set-input inbuf + 2+ k buffer-cpy add-crlf 2drop
  \ inbuf 17 ." INBUF:" dump cr
  get-response 11 =always drop
  resp resp-len @ s string-eq -1 =always
empty-stack

cr test-summary
