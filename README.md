f-t-w is a peer to peer encrypted cli messenger over TCP/IP.

messages are encrypted with AES-256 using a shared secret through Kyber KEM (quantum resistent!)

credit to The Bouncy Castle on Kyber package: https://github.com/bcgit/bc-csharp

##### to-do:
- Improve code clarity, restructure to make more legible
- Build 'discovery' server to host username -> IP mapping, allowing a hub for users to connect with another at a higher level than IP

- Use HTTP3, fallback to HTTP2(or 1.1) to account for Apple not enabling by
  default
  - This is to switch to UDP hole punching to establish connections, may need to read through this again (https://tailscale.com/blog/how-nat-traversal-works/)

- Test IPv6 to IPv6, recently I switched to a home 5G Gateway (T-Mobile internet) and they do not support portforwarding or UPnP