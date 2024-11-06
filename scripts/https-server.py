import http.server
import ssl
import os
import mimetypes
from http import HTTPStatus

class BrotliHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header("Cache-Control", "no-store")
        is_br = self.path.endswith(".br")
        if is_br:
            self.send_header("Content-Encoding", "br")
        super().end_headers()

    def guess_type(self, path):
        content_type, _ = mimetypes.guess_type(path)
        return content_type or "application/octet-stream"

if __name__ == "__main__":
    script_dir = os.path.dirname(os.path.abspath(__file__))
    cert_file = os.path.join(script_dir, "../certs/cert.pem")
    key_file = os.path.join(script_dir, "../certs/key.pem")

    port = 8443
    server_address = ("127.0.0.1", port)
    httpd = http.server.HTTPServer(server_address, BrotliHTTPRequestHandler)

    # Configure SSL context
    context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
    context.load_cert_chain(certfile=cert_file, keyfile=key_file)
    httpd.socket = context.wrap_socket(httpd.socket, server_side=True)

    print(f"Serving on https://127.0.0.1:{port}")
    httpd.serve_forever()
