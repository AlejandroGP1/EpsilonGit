import cv2
import mediapipe as mp
import time
import math
import socket

# --- CONFIGURACIÓN DEL SOCKET ---
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5052)

# Alias de MediaPipe Tasks
BaseOptions = mp.tasks.BaseOptions
HandLandmarker = mp.tasks.vision.HandLandmarker
HandLandmarkerOptions = mp.tasks.vision.HandLandmarkerOptions
VisionRunningMode = mp.tasks.vision.RunningMode

CONNECTIONS = [
    (0, 1), (1, 2), (2, 3), (3, 4), (0, 5), (5, 6), (6, 7), (7, 8),
    (9, 10), (10, 11), (11, 12), (13, 14), (14, 15), (15, 16),
    (17, 18), (18, 19), (19, 20), (5, 9), (9, 13), (13, 17), (0, 17)
]


def DeteccionDeGestos(hand):
    # --- Estados de los dedos ---
    indice_arriba  = hand[8].y < hand[6].y
    corazon_arriba = hand[12].y < hand[10].y
    anular_arriba  = hand[16].y < hand[14].y
    menique_arriba = hand[20].y < hand[18].y
    
    # --- Lógica de Palma Abierta (Filtro de seguridad) ---
    # Si todos los dedos están extendidos, es Palma Abierta.
    if indice_arriba and corazon_arriba and anular_arriba and menique_arriba:
        return "Palma Abierta"

    if indice_arriba and menique_arriba and not corazon_arriba and not anular_arriba:
        return "Rock"

    # --- Nueva validación: ¿Está el índice extendido? ---
    dist_indice_base = math.dist([hand[8].x, hand[8].y], [hand[5].x, hand[5].y])
    dist_nudillo_base = math.dist([hand[6].x, hand[6].y], [hand[5].x, hand[5].y])
    indice_extendido = dist_indice_base > dist_nudillo_base * 1.2
    
    # --- Lógica de Dirección del Índice ---
    dx = hand[8].x - hand[6].x
    dy = hand[8].y - hand[6].y
    
    if abs(dx) > abs(dy):
        direccion_indice = "Indice a la Derecha" if dx > 0 else "Indice a la Izquierda"
    else:
        direccion_indice = "Indice Arriba" if dy < 0 else "Indice Abajo"

    # --- Lógica de Gestos Especiales ---
    dist_ok = math.dist([hand[4].x, hand[4].y], [hand[8].x, hand[8].y])
    
    # Saludo (Solo índice y corazón, anular abajo)
    if indice_arriba and corazon_arriba and not anular_arriba: 
        return "Saludo"
        
    if dist_ok < 0.05 and corazon_arriba: 
        return "Ok"
        
    if not indice_arriba and not corazon_arriba and not anular_arriba: 
        return "Back"
        
    if menique_arriba and not indice_arriba and not corazon_arriba and not anular_arriba: 
        return "Promesa"
    
    if indice_extendido:
        return direccion_indice
        
    return "Nada"

def main():
    options = HandLandmarkerOptions(
        base_options=BaseOptions(model_asset_path='hand_landmarker.task'),
        running_mode=VisionRunningMode.VIDEO,
        num_hands=1,
        min_hand_detection_confidence=0.7,
        min_hand_presence_confidence=0.7,
        min_tracking_confidence=0.7
    )

    cap = cv2.VideoCapture(0)
    
    if not cap.isOpened():
        sock.sendto(b"Sin Camara Disponible|Nada|0|0", serverAddressPort)
        print("Error: Camara no detectada")
        return

    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 960)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 540)

    with HandLandmarker.create_from_options(options) as landmarker:
        while cap.isOpened():
            success, frame = cap.read()
            if not success:
                sock.sendto(b"Sin Camara Disponible|Nada|0|0", serverAddressPort)
                break

            frame = cv2.flip(frame, 1)
            h, w, _ = frame.shape
            rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)
            timestamp = int(time.time() * 1000)
            
            result = landmarker.detect_for_video(mp_image, timestamp)

            # Variable para almacenar lo que vamos a enviar y mostrar
            mensaje_socket = ""

            if result.hand_landmarks:
                hand = result.hand_landmarks[0]
                
                for conn in CONNECTIONS:
                    x1, y1 = int(hand[conn[0]].x * w), int(hand[conn[0]].y * h)
                    x2, y2 = int(hand[conn[1]].x * w), int(hand[conn[1]].y * h)
                    cv2.line(frame, (x1, y1), (x2, y2), (0, 255, 0), 2)
                for lm in hand:
                    cv2.circle(frame, (int(lm.x*w), int(lm.y*h)), 5, (0, 0, 255), -1)

                gesto = DeteccionDeGestos(hand)
                posX, posY = hand[9].x, hand[9].y
                mensaje_socket = f"EXISTO|{gesto}|{posX:.3f}|{posY:.3f}"
            else:
                mensaje_socket = "Sin Mano detectada|Nada|0|0"

            # 1. Enviar por Socket
            sock.sendto(mensaje_socket.encode(), serverAddressPort)

            # 2. Mostrar en pantalla (Esquina inferior izquierda)
            # Dibujamos un pequeño rectángulo de fondo para legibilidad
            cv2.rectangle(frame, (0, h - 35), (w, h), (0, 0, 0), -1) 
            cv2.putText(frame, f"SEND: {mensaje_socket}", (10, h - 10), 
                        cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 255), 1, cv2.LINE_AA)

            cv2.imshow('Epsilon Pro', frame)
            if cv2.waitKey(1) & 0xFF == 27: break
            if cv2.getWindowProperty("Epsilon Pro", cv2.WND_PROP_VISIBLE) < 1: break

    cap.release()
    cv2.destroyAllWindows()

if __name__ == "__main__":
    main()