```
  ______             __                           
 /      \           /  |                          
/$$$$$$  |         _$$ |_            __   __   __ 
$$ |_ $$/  ______ / $$   |   ______ /  | /  | /  |
$$   |    /      |$$$$$$/   /      |$$ | $$ | $$ |
$$$$/     $$$$$$/   $$ | __ $$$$$$/ $$ | $$ | $$ |
$$ |                $$ |/  |        $$ \_$$ \_$$ |
$$ |                $$  $$/         $$   $$   $$/ 
$$/                  $$$$/           $$$$$/$$$$/
```

http://patorjk.com/software/taag/#p=display&h=0&f=Big%20Money-sw&t=f-t-w

f-t-w is a peer to peer encrypted cli messenger over TCP.

messages are encrypted with AES using a derived key generated by peers through
Diffie-Hellman

##### to-do:

- Use HTTP3, fallback to HTTP2(or 1.1) to account for Apple not enabling by
  default
- Switch out Diffie-Hellman in favor of Kyber KEM (post-quantum KEM approved by
  NIST)
