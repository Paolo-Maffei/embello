\ HC-05 (/HC-06) bluetooth module driver
\ The module is expected to be attached to uart2. The driver implements primitives to
\ initially configure the module in slave mode (i.e. so a phone can discover it and pair
\ with it) as well as primitives to send and receive messages.

\ bug in mecrisp compare!?
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

10 constant LF
13 constant RET
0 variable resp-len
100 buffer: resp
100 buffer: line

: not-lf? ( c -- c f ) LF <> ;
: is-cr? ( c -- f ) RET = ;
: save-char ( i c -- i ) over line + c! 1+ ;
: strip-cr ( n -- n )
  dup if
    dup line + 1- c@ ( n c ) \ fetch last char
    is-cr? if 1- then    \ drop last char
  then ;
: read-line ( -- n ) \ read 'til LF, strip CR
  uart2-key? not if
    100 ms uart2-key? not if
      0 exit
    then
  then
  0 begin ( i )
    uart2-key dup not-lf? while
    save-char
  repeat ( i )
  drop strip-cr ( n )
  ;
: not-OK? ( n -- n ) \ returns 0 if line is "OK"
  s" OK" line 3 pick string-eq ( n f )
  if drop 0 then ( n )
  ;

: .string ( c-addr len -- ) \ print string in "
  [char] " emit  type  [char] " emit ;

: get-response ( -- c-addr len ) \ wait for OK\r\n, return previous line
  0 resp-len !
  begin
    read-line \ ." GOT:" line over .string cr
    not-OK? ?dup while
    dup resp-len !
    line resp rot move
  repeat
  resp resp-len @
  ;

: check-err ( -- f ) \ check for OK response, if not leave in resp/resp-len
  read-line not-OK? dup if
    line resp rot move
  then ;

: send-string ( c-addr len -- )
  0 ?do dup i + c@ uart2-emit loop drop ;
: send-line ( c-addr len -- )
  send-string RET uart2-emit LF uart2-emit ;

: test-presence ( -- f ) \ test that AT returns OK
  begin uart2-key? while uart2-key drop repeat
  s" AT" send-line
  0
  read-line ?dup if
    ." GOT: " line over .string cr
    s" OK" rot line swap string-eq 
    if drop -1 then
  then ;

: get-version ( -- f )
  s" AT+VERSION?" send-line
  get-response nip 9 >
  ;

: .version ( -- )
  get-version if resp resp-len @ .string cr else ." error" cr then ;

: set-name ( c-addr len -- )
  s" AT+NAME=" send-string send-line
  check-err if ." ERR: set name: " resp resp-len @ .string cr then ;

\ : .name ( -- ) \ return name of device, doesn't seem to work
\   s" AT+NAME?" send-line
\   get-response ?dup if .string cr else drop ." cannot get name" cr then ;

: set-baud ( c-addr len -- )
  s" AT+UART=" send-string send-string
  s" ,0,0" send-line
  check-err if ." ERR: set uart " resp resp-len @ .string cr then ;

: .baud ( -- ) \ return baud rate info
  s" AT+UART?" send-line
  get-response ?dup if .string cr else drop ." cannot get baud rate" cr then ;


: hc05:config
  uart2-init 38400 uart2-baud
  test-presence if
    ." Found HC-05" cr
    .version
    s" rf-tracker" set-name
    s" 38400" set-baud .baud
  else
    cr ." No HC-05" cr
  then ;

: hc05:run s" AT+RESET" send-line ;
  
