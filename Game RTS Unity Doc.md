# Game RTS Unity — Tài liệu thiết kế & Lộ trình (làm một mình)

> Kế hoạch triển khai đang cập nhật. Bản Claude nguyên văn chỉ để đọc và đối chiếu nằm trong `TAI_LIEU_GOC_CLAUDE.txt`. Luật làm việc ở `role/LUAT_LAM_VIEC.md`; danh mục file ở `role/CAU_TRUC_DU_AN.md`.

Phong cách: Age of Empires / War of Empire Conquest · Engine: Unity · Nhân sự: 1 người

## 1. Tổng quan

Đây là game chiến thuật thời gian thực (RTS). Người chơi thu thập tài nguyên, xây căn cứ, huấn luyện quân, nâng cấp và phá hủy căn cứ đối phương.

- Chế độ chơi: (1) Đánh với máy (AI), (2) Đánh với người thật (online).

- Bản đồ: 3–5 map, mỗi map có địa hình và cách bố trí tài nguyên khác nhau.

- Tài nguyên: Vàng, Thực (thịt), Gỗ.

- Quân: lính giáo, kị binh, cung thủ, pháp sư, xe công thành, lính lâu đài.

- Công trình: xưởng thực, xưởng vàng, xưởng gỗ; phòng thủ gồm trạm soi map, tháp cung, bom, tường.

## 2. Nguyên tắc khi làm một mình

Làm RTS một mình là khả thi nếu giữ phạm vi nhỏ và đi theo từng lát dọc.

1. Làm bản chơi được trước, mở rộng sau. Mục tiêu đầu tiên: 1 map, 3 loại quân, đánh được AI từ đầu đến khi thắng.

2. Chế độ đánh với máy là sản phẩm chính. Chế độ online là mục tiêu mở rộng, làm sau cùng và chỉ khi bản đánh máy đã ổn.

3. Dữ liệu tách khỏi code. Chỉ số quân, chi phí, nâng cấp lưu trong ScriptableObject để chỉnh cân bằng không cần sửa code.

4. Dùng asset có sẵn (Unity Asset Store, Kenney.nl, itch.io, OpenGameArt) cho đồ họa và âm thanh trong giai đoạn đầu.

5. Mô phỏng tách khỏi hiển thị ngay từ đầu để sau này thêm multiplayer không phải viết lại.

6. Cắt tính năng sớm. Mỗi tính năng thêm vào là thêm thời gian cân bằng và sửa lỗi. Chỉ giữ thứ giúp game vui hơn.

## 3. Luật chơi cốt lõi

Vòng lặp: Thu thập tài nguyên → Xây dựng → Tạo quân → Nâng cấp → Giao tranh → Phá căn cứ địch.

Điều kiện thắng: phá hủy Tòa Thị Chính của đối phương. Bản đầu chỉ dùng điều kiện này cho đơn giản.

Khởi đầu mỗi ván: một Tòa Thị Chính, một số dân (ví dụ 3–5), một lượng tài nguyên ban đầu nhỏ.

Giới hạn dân số: mỗi quân và dân chiếm một số chỗ. Xây Nhà để tăng giới hạn, có trần tối đa chung (ví dụ 100–150 ở bản đầu). Nó vừa chặn việc spam quân, vừa giữ hiệu năng game ổn.

## 4. Tài nguyên

| Tài nguyên | Nguồn | Cách thu | Nộp tại |
| --- | --- | --- | --- |
| Gỗ | Cây | Dân chặt; cây cạn dần rồi biến mất | Xưởng gỗ |
| Thực | Bụi quả, thú (nai, trâu, voi), cá, quái | Dân hái/săn/câu; voi và quái cần quân đánh | Xưởng thực |
| Vàng | Mỏ đá vàng | Dân đào | Xưởng vàng |

Luật bổ sung:

- Mọi nguồn có trữ lượng hữu hạn (ví dụ cây 100 gỗ, mỏ vàng 800). Hết thì phải mở rộng, và đây là gốc của chiến thuật.

- Dân mang tối đa N tài nguyên (10–20) rồi phải về xưởng nộp. Vị trí đặt xưởng gần nguồn vì thế rất quan trọng.

- Thú có độ nguy hiểm khác nhau: nai/trâu hiền (dân săn được), voi/quái mạnh (cần quân hạ, thưởng nhiều thực).

- Ruộng (Farm): nguồn thực vô hạn nhưng thu chậm, tránh bị kẹt khi hết thú.

- Câu cá: chỉ cần ở map có nước. Bản đầu cho dân đứng bờ câu, không làm thuyền.

- Chợ đổi tài nguyên: để dành làm sau, không cần ở bản đầu.

Chi phí nâng cấp: mỗi nâng cấp tốn 1, 2 hoặc cả 3 loại tài nguyên tùy mức. Nâng cấp càng cao càng cần nhiều loại.

## 5. Quân đội

### 5.1 Tam giác khắc chế

- Lính giáo khắc Kị binh.

- Kị binh khắc Cung thủ và Pháp sư.

- Cung thủ khắc Lính giáo (bắn xa trước khi giáo kịp tiến).

- Pháp sư gây sát thương lan, mạnh với đám đông, máu thấp.

- Xe công thành mạnh với công trình, yếu với mọi quân cận chiến.

- Lính lâu đài là quân tinh nhuệ, rất đắt, tạo từ Lâu đài.

### 5.2 Bảng quân (số liệu để chỉnh sau khi chơi thử)

| Quân | Tạo ở | Máu | Tầm | Điểm mạnh | Điểm yếu | Chi phí gợi ý |
| --- | --- | --- | --- | --- | --- | --- |
| Dân | Tòa thị chính | Thấp | Rất gần | Thu hoạch, xây dựng | Gần như không đánh được | Thực |
| Lính giáo | Trại lính | Trung bình | Gần | Bền, rẻ, thắng kị | Chậm, thua cung | Thực + Gỗ |
| Kị binh | Chuồng ngựa | Cao | Gần | Nhanh, đánh úp | Thua giáo, đắt | Thực + Vàng |
| Cung thủ | Trường bắn | Thấp | Xa | Sát thương tầm xa | Yếu khi bị áp sát | Gỗ + Vàng |
| Pháp sư | Tháp phép | Rất thấp | Trung bình | Sát thương lan | Dễ chết | Vàng + Thực |
| Xe công thành | Xưởng công thành | Cao | Gần | Phá công trình | Không tự vệ | Gỗ + Vàng |
| Lính lâu đài | Lâu đài | Rất cao | Gần | Quân tinh nhuệ | Rất đắt, số lượng ít | Cả 3 |

### 5.3 Chỉ số mỗi quân

HP, sát thương, tốc độ đánh, tầm đánh, tốc độ di chuyển, giáp, tầm nhìn, số dân chiếm, thời gian huấn luyện.

Loại sát thương: cận chiến, tầm xa, phép, công thành. Giáp giảm từng loại theo mức khác nhau.

### 5.4 Lệnh cho quân (bản đầu)

Di chuyển, tấn công, dừng. Sau đó có thể thêm: giữ vị trí, tuần tra, tập hợp (rally point) cho công trình tạo quân.

## 6. Công trình và thời đại

### 6.1 Công trình kinh tế và quân sự

- Tòa Thị Chính: tạo dân, nơi nộp tài nguyên, tự bắn yếu khi bị đánh.

- Nhà: tăng giới hạn dân số.

- Xưởng thực, xưởng vàng, xưởng gỗ: nơi nộp tài nguyên tương ứng, nâng cấp tốc độ thu hoạch.

- Ruộng: nguồn thực vô hạn.

- Trại lính, Chuồng ngựa, Trường bắn, Tháp phép, Xưởng công thành, Lâu đài: tạo quân tương ứng.

### 6.2 Thời đại (để tạo nhịp độ ván chơi)

- Thời đại 1: Tòa thị chính, nhà, 3 xưởng, trại lính. Lính giáo và cung thủ.

- Thời đại 2: Chuồng ngựa, tháp phép; nâng cấp quân và công trình.

- Thời đại 3: Lâu đài, xe công thành, lính lâu đài.

Chuyển thời đại tốn tài nguyên (có lần 1 loại, có lần cả 3). Với bản đầu, có thể gộp thành 2 thời đại cho gọn.

## 7. Công trình phòng thủ

| Công trình | Chức năng | Ghi chú thiết kế |
| --- | --- | --- |
| Trạm soi map | Mở rộng tầm nhìn | Gắn với sương mù chiến tranh |
| Tháp bắn cung | Bắn tự động vào địch | Nâng cấp được tầm và sát thương; có thể cho quân vào trú |
| Bom | Nổ khi địch tới gần | Dùng một lần; nên vô hình với địch |
| Tường | Chặn đường | Cần thêm cổng cho quân mình đi qua; địch phải phá hoặc đi vòng |

## 8. Sương mù chiến tranh

- Bản đồ ban đầu đen (chưa khám phá).

- Vùng đã đi qua nhưng không còn thấy thì mờ: nhớ địa hình, không thấy quân địch.

- Chỉ thấy quân và công trình địch trong tầm nhìn của đơn vị hoặc trạm soi.

- Gợi ý kỹ thuật: có thể làm đơn giản bằng lưới ô nhỏ và texture che; không cần giải pháp phức tạp.

## 9. Bản đồ (3–5 map)

| Map | Địa hình | Đặc điểm chiến thuật |
| --- | --- | --- |
| 1. Đồng cỏ (dễ, làm trước) | Phẳng, ít vật cản | Tài nguyên dồi dào, học cơ bản |
| 2. Rừng rậm | Nhiều cây, đường hẹp | Gỗ nhiều, vàng ít, dễ phục kích |
| 3. Sa mạc / núi đá | Mỏ vàng nhiều, ít cây | Thiếu gỗ, phải tranh mỏ |
| 4. Hồ / sông | Có nước, eo đất hoặc cầu | Có cá, đường đi bị chia cắt |
| 5. Trung tâm tranh chấp | Khu giữa giàu tài nguyên | Buộc giao tranh sớm |

Mỗi map cần: vị trí xuất phát cân bằng cho các bên, số lượng và vị trí tài nguyên, vật cản, số người chơi hỗ trợ (1v1 trước, 2v2 sau nếu cần).

Khuyến nghị cho solo: làm map 1 hoàn chỉnh trước, sau đó chỉ cần thay địa hình và đặt lại tài nguyên cho các map còn lại. Nếu hệ thống đặt tài nguyên được thiết kế tốt, mỗi map mới tốn ít công.

## 10. Hai chế độ chơi

### 10.1 Đánh với máy (ưu tiên số 1)

- 3 độ khó: Dễ, Thường, Khó.

- AI bản đầu là máy trạng thái: phát triển kinh tế → xây quân → tấn công theo đợt → rút lui khi yếu.

- AI khó không nên gian lận tài nguyên ở bản đầu; chỉ cần phản ứng nhanh hơn và quyết định tốt hơn.

### 10.2 Đánh với người (làm sau cùng)

Đã chốt deterministic lockstep ngày 11/11/2026 cho multiplayer trên PC. Các máy bắt đầu từ cùng trạng thái, bản đồ, dữ liệu game và seed, nhận cùng tập lệnh theo tick rồi tự mô phỏng toàn bộ trận đấu. Dữ liệu đồng bộ thường xuyên gồm lệnh và thông tin điều phối. Trạng thái đầy đủ có thể dùng cho khởi tạo hoặc phục hồi khi có thiết kế hỗ trợ.

- Simulation chạy theo tick cố định, độc lập FPS. Mỗi máy chỉ xử lý tick khi đã có đầy đủ tập lệnh hoặc xác nhận không có lệnh của các bên cho tick đó. Khi thiếu dữ liệu, chờ dữ liệu theo cơ chế lockstep. Độ trễ đầu vào được đệm để xử lý dao động mạng. Tần số tick và độ đệm sẽ được chọn sau khi đo bản thử.

- Bảo đảm deterministic từ mốc 1. Tính toán ảnh hưởng luật chơi dùng số nguyên hoặc fixed-point với quy tắc làm tròn và xử lý tràn thống nhất. Kiểm soát thứ tự cập nhật đơn vị, duyệt tập hợp, chọn mục tiêu và xử lý các trường hợp hòa khi tìm đường. Unity Physics và NavMesh không quyết định trạng thái mô phỏng. View có thể dùng số thực để hiển thị.

- Ngẫu nhiên của Simulation và AI dùng bộ sinh số xác định với seed chung cùng thứ tự gọi. Ngẫu nhiên trang trí chỉ thuộc View. AI chạy theo tick và phải tạo cùng quyết định trên mọi máy.

- Mọi hành động ảnh hưởng luật chơi đi qua Command. Mỗi lệnh có tick thực thi, mã người chơi và số thứ tự. Thứ tự xử lý được quy định thống nhất. Simulation kiểm tra quyền điều khiển và điều kiện thực thi giống nhau trên mọi máy. Cần nhận diện lệnh trùng và dữ liệu đến muộn khi triển khai truyền qua mạng.

Định kỳ so sánh checksum trạng thái để phát hiện desync. Khi desync, dừng hoặc báo lỗi với thông tin tick và lệnh phục vụ chẩn đoán. Nếu dùng host để điều phối, host sắp lịch và chuyển tiếp tập lệnh, còn mỗi máy vẫn chạy cùng Simulation. Topology kết nối, thư viện mạng và dịch vụ relay sẽ được lựa chọn sau. Lockstep tiết kiệm lưu lượng đồng bộ, còn chi phí dịch vụ kết nối phụ thuộc cách triển khai.

## 11. Giao diện (UI)

- Thanh tài nguyên (gỗ, thực, vàng) và dân số hiện tại/giới hạn.

- Minimap.

- Khung thông tin đơn vị và bảng lệnh (xây, huấn luyện, nâng cấp).

- Chọn quân: click, kéo khung, nhóm bằng Ctrl+1…9.

- Thông báo (đang bị tấn công, hết tài nguyên, xong nâng cấp).

- Menu chính, chọn map và độ khó, cài đặt, màn hình thắng/thua.

## 12. Rủi ro chính

1. Multiplayer realtime rất khó và dễ nuốt thời gian của người làm một mình. Đặt nó là mục tiêu mở rộng, không phải điều kiện của bản đầu.

2. Pathfinding nhiều quân: dễ kẹt, chen chúc. Cần thử nghiệm sớm với 50–100 quân.

3. Hiệu năng: hàng trăm đơn vị cần object pooling và tối ưu tính toán cho quân ở xa.

4. Cân bằng game: tốn nhiều thời gian chơi thử. Giữ số lượng quân và nâng cấp ít để dễ cân bằng.

5. Phình phạm vi: mỗi quân hay công trình mới kéo theo animation, âm thanh, UI và cân bằng.

6. Kiệt sức khi làm một mình: chia nhỏ thành các mốc có thể chơi được để luôn thấy tiến triển.

## 13. Kiến trúc kỹ thuật gợi ý (Unity)

- Phiên bản đã thiết lập ngày 11/11/2026. Unity 6.3 LTS 6000.3.13f1, project Universal 2D tại GameRTS. Input System 1.14.0. Dùng Git để quản lý project cùng tài liệu gốc.

- Dữ liệu: ScriptableObject cho quân, công trình, nâng cấp, map.

- Đã chốt Simulation/View và deterministic lockstep. Simulation quản lý trạng thái cùng luật chơi bằng C# độc lập với MonoBehaviour, GameObject, SpriteRenderer và thiết bị nhập. Simulation chạy theo tick cố định với phép tính, thứ tự xử lý và ngẫu nhiên xác định ngay từ mốc 1. View đọc trạng thái để cập nhật sprite, animation, giao diện và hiệu ứng, có thể nội suy hình ảnh giữa các tick.

- Đã chốt hệ thống Command. Input và AI tạo lệnh dùng chung như MoveCommand, AttackCommand, GatherCommand, BuildCommand và TrainCommand. Command mang tick thực thi, mã người chơi, số thứ tự và dữ liệu mục tiêu đã chuẩn hóa bằng mã hoặc tọa độ mô phỏng. Simulation kiểm tra và thực thi lệnh theo thứ tự xác định. Việc chọn quân và kéo camera thuộc lớp điều khiển và hiển thị.

- Pathfinding 2D đã có bản nền Mốc 1 bằng lưới và A\* xác định, cập nhật từ bốn sang tám hướng ngày 13/11/2026. Chi phí cạnh và chéo lần lượt là 1000 và 1414; ước lượng theo khoảng cách ngang, dọc và chéo với quy tắc phân xử thống nhất. Bước chéo chỉ được dùng khi cả hai ô sát góc đều đi được. Simulation dùng cùng bản đồ và dữ liệu vật cản trên mọi máy, quản lý ô đích riêng và giữ chỗ cho từng quân. Khi triển khai công trình và nguồn tài nguyên, bổ sung cập nhật ô đi được bằng luồng trạng thái/lệnh xác định. Thuật toán đã thử với 100 quân; tiếp tục kiểm tra khi thêm loại quân và vật cản động.

- Tối ưu: object pooling cho đạn, hiệu ứng và quân; cập nhật AI/mục tiêu theo nhịp thay vì mỗi frame.

- Kiến trúc mạng đã chốt là deterministic lockstep. Lõi mô phỏng xác định được làm từ mốc 1. Lobby, kết nối và truyền lệnh triển khai tại mốc multiplayer. Lựa chọn thư viện truyền mạng dựa trên khả năng gửi tập lệnh theo tick và yêu cầu kết nối, sau bản thử 1v1.

## 14. Lộ trình theo mốc (tối ưu cho một người)

Mỗi mốc kết thúc bằng một thứ chơi được. Thời gian là ước lượng thô, tùy vào việc làm toàn thời gian hay bán thời gian.

### Mốc 0 — Chuẩn bị hoàn thành ngày 11/11/2026

- Đã chốt nền tảng phát hành ngày 11/11/2026 là PC. Phát triển và chạy thử trong Unity Editor bằng chuột và bàn phím. Android xem xét sau. Tách lớp Input và dùng chung hệ thống Command để thuận tiện mở rộng điều khiển nếu bổ sung nền tảng về sau.

- Đã chốt game 2D top-down với camera cố định nhìn từ trên xuống. Quân, nhà và tài nguyên dùng sprite 2D có góc vẽ thống nhất với camera.

- Đã tạo project GameRTS từ template Universal 2D bằng Unity 6000.3.13f1, thiết lập Git tại thư mục gốc để theo dõi project, AGENTS.md và tài liệu này. Cấu hình Assembly Definition tách Simulation, Commands, View và Input. Simulation và Commands không tham chiếu UnityEngine.

- Đã chốt kiến trúc Simulation/View, Command và deterministic lockstep. Offline dùng cùng lõi Simulation và luồng lệnh theo tick. Khi làm multiplayer, bổ sung lớp truyền và điều phối tập lệnh giữa các máy, cùng kiểm tra desync. Tính xác định của di chuyển, tìm đường, AI và giao tranh được kiểm tra từ các mốc tương ứng.

- Cấu trúc hiện tại trong Assets/Game gồm Simulation, Commands, View, Input, Data, Scenes, Art và UI. Các lớp Simulation và Commands không phụ thuộc lớp View hoặc Input. Dữ liệu cấu hình bằng ScriptableObject được chuyển sang dữ liệu mô phỏng khi khởi tạo ván.

- Kiểm thử bản PC trong Unity Editor và bằng bản build PC tại các mốc có thể chơi được. Từ mốc 1, chạy hai bản Simulation với cùng seed và tập lệnh, đối chiếu checksum theo tick. Khi có build PC, kiểm tra tính xác định giữa Editor và build. Kiểm thử mạng thật và độ trễ thực hiện khi triển khai multiplayer. Android xem xét sau.

### Mốc 0 đã hoàn thành ngày 11/11/2026. Đã chốt nền tảng phát hành PC, góc nhìn 2D top-down, Simulation/View, Command và deterministic lockstep. Đã thiết lập project Universal 2D với Unity 6000.3.13f1, cấu trúc thư mục, Git và repo GitHub. Người dùng xác nhận Play trong Editor chạy được. Đã build GameScene thành công cho Windows Intel 64-bit với Mono trong 170 giây. Bản chạy nằm tại Builds/Windows/GameRTS.exe cùng dữ liệu đi kèm. Đã mở bản thực thi ngoài Editor, quan sát cửa sổ GameRTS hiển thị nền xanh của scene ban đầu và xác nhận tiến trình phản hồi bình thường. Log runtime không ghi nhận exception hoặc crash. Đây là scene nền với camera, gameplay bắt đầu ở mốc 1. Lõi mô phỏng xác định được triển khai từ mốc 1, chức năng multiplayer ở mốc 8.

### Mốc 1 — Nền móng hoàn thành ngày 13/11/2026

- Camera RTS đã triển khai ngày 12/11/2026. RtsCameraInput ở Assets/Game/Input đọc WASD, phím mũi tên, con lăn và edge scrolling trong dải 16 pixel sát mép viewport. RtsCameraView ở Assets/Game/View xử lý pan, zoom và clamp toàn bộ vùng nhìn vào map. Tốc độ pan mặc định 20 đơn vị/giây, hướng chéo được chuẩn hóa. Zoom Orthographic mặc định 10, Min 4, Max 20, độ nhạy 1,5 theo một đơn vị scroll đã chuẩn hóa của Input System. Khi viewport quá rộng hoặc map nhỏ, mức zoom được điều chỉnh để vẫn nằm trong biên map.

- Bản đồ 2D cơ bản đã triển khai ngày 12/11/2026 trong GameScene. Map Grid dùng ô vuông 1 x 1, có Tilemap Ground gồm 64 x 64 ô, tọa độ từ (-32, -32) đến (31, 31), biên thế giới từ -32 đến 32 trên mỗi trục. Sprite GroundTile.png kích thước 32 x 32 pixel với Pixels Per Unit bằng 32. Hai Tile asset GrassLight và GrassDark trong Assets/Game/Art/Terrain được xếp xen kẽ theo mảng 4 x 4, TilemapRenderer sorting order -10. Ngày 13/11/2026 thêm Tilemap Obstacles dùng RockObstacle.asset, sorting order -5. RtsMapView chuyển ô có nền và không có vật cản thành ô đi được của Simulation khi khởi tạo ván.

- Camera thuộc View và Input, điều khiển góc nhìn cục bộ và không đưa pan hoặc zoom vào lệnh lockstep. Trong Editor, input camera chỉ hoạt động khi Game View có focus, edge scrolling và zoom chỉ nhận khi chuột nằm trong Game View. Khi mất focus hoặc chuột ra ngoài viewport, camera không tiếp tục pan theo vị trí chuột cũ.

- Kiểm tra ngày 12/11/2026. 11/11 test EditMode đã qua, bao gồm WASD và phím mũi tên với thiết bị bàn phím ảo, phím đối nhau, tốc độ pan và hướng chéo, giới hạn zoom, clamp bốn góc viewport, màn hình dọc và rất rộng, map nhỏ. Đã quan sát map trong Play Mode, xác nhận edge scrolling đưa camera tới đúng biên và con lăn thay đổi zoom. Bản Windows 64-bit build thành công với 0 lỗi và khởi động được. Test nằm trong Assets/Game/Tests/Editor.

- Cách chỉnh nền trong Unity. Mở Window \> 2D \> Tile Palette, tạo một palette và kéo GrassLight cùng GrassDark từ Assets/Game/Art/Terrain vào palette. Chọn Ground trong Hierarchy rồi dùng cọ để tô. Giữ map dạng hình chữ nhật theo trục XY khi dùng cơ chế clamp hiện tại. Có thể thay sprite trong Tile asset để đổi hình nền.

- Dữ liệu đơn vị và spawn thử nghiệm đã triển khai ngày 13/11/2026. UnitDataSO ở Assets/Game/Data là ScriptableObject chứa UnitId, DisplayName, MoveSpeed, CollisionRadius và ViewPrefab. Asset SampleSoldierData trong Assets/Game/Data/Units có ID loại sample\_soldier, tên Lính mẫu, tốc độ mẫu 3 đơn vị/giây, bán kính mẫu 0,45 và tham chiếu SampleSoldier.prefab. Các số liệu này phục vụ thử nghiệm và chưa phải cân bằng cuối cùng.

- Prefab SampleSoldier ở Assets/Game/View/Prefabs gồm SpriteRenderer và UnitView ở gốc, cùng đối tượng con SelectionCircle có SpriteRenderer riêng. Sprite quân và vòng chọn là ảnh PNG nền trong suốt 64 x 64 pixel trong Assets/Game/Art/Units, Pixels Per Unit bằng 64, lọc Point. Thân quân có sorting order 10, vòng chọn 5 và nền map -10. Vòng chọn mặc định tắt; UnitView.SetSelected bật hoặc tắt vòng. UnitView.Render nội suy vị trí từ hai trạng thái tick của Simulation để hiển thị di chuyển.

- GameScene có Unit Preview với UnitPreviewSpawner và RTS Session với RtsGameView, UnitSelectionView, UnitSelectionInput. Khi Play, spawner chuyển SampleSoldierData cùng 5 Spawn Positions cho RtsGameView. SimulationWorld cấp ID, tìm ô spawn trống gần vị trí cấu hình, sau đó View tạo Prefab tương ứng. Các vị trí cấu hình là (-4; -1,5), (-2; 1), (0; 0), (2; 1), (4; -1,5); trạng thái ban đầu được đặt ở tâm ô trống. Trong GameScene không chọn sẵn quân. Select First For Preview chỉ dùng khi xem Prefab độc lập và không gán Game cho spawner.

- Phân tách trách nhiệm ở Mốc 1: ScriptableObject chứa cấu hình, RtsGameView chuẩn hóa chỉ số theo tỷ lệ 1000 đơn vị số nguyên bằng 1 đơn vị thế giới và khởi tạo SimulationWorld. Simulation cấp ID, giữ UnitState, thực thi MoveCommand và quản lý đường đi cùng giữ chỗ va chạm. Simulation và Commands là C# thuần, không tham chiếu UnityEngine hoặc dùng số thực. View nội suy hình ảnh bằng số thực. Chọn quân và camera là trạng thái cục bộ; lệnh di chuyển chứa dữ liệu số nguyên để có thể truyền qua mạng về sau.

- Cách chỉnh và tạo loại quân: chọn Assets/Game/Data/Units/SampleSoldierData trong Project để sửa Inspector. Tạo asset mới bằng Create \> GameRTS \> Unit Data, đặt ID loại riêng, tên, các chỉ số và View Prefab có UnitView ở gốc. Trong Hierarchy, Unit Preview cho phép đổi Unit Data, số lượng và tọa độ Spawn Positions. GameScene hiện kết nối Game với RTS Session để Simulation tạo trạng thái; tùy chọn chọn sẵn chỉ phục vụ xem Prefab độc lập. Thay sprite của Prefab khi có hình chính thức. Mốc 1 yêu cầu bán kính sau chuẩn hóa từ 1 đến 499, tức nhỏ hơn 0,5 đơn vị thế giới, và tốc độ từ 0 đến 20 đơn vị/giây; quân có tốc độ 0 không nhận lệnh di chuyển.

- Kiểm tra bước dữ liệu và spawn ngày 13/11/2026: tổng 17/17 test EditMode đã qua, gồm 11 test camera và 6 test đơn vị. Các test đơn vị kiểm tra Prefab, vòng chọn, khởi tạo lại, ID và dữ liệu dùng chung của 5 bản sao, đầu vào không hợp lệ và chuẩn hóa cấu hình. Play Mode thực tế xác nhận 5 quân, 5 ID riêng, 1 vòng chọn bật; đã quan sát quân trên map. Bản Windows 64-bit build thành công với 0 lỗi. Bước dữ liệu và spawn thử nghiệm hoàn thành trước khi kết nối chọn quân và Simulation.

- Chọn quân hoàn thành ngày 13/11/2026. Chuột trái chọn một quân; click nền bỏ chọn; giữ và kéo chuột trái chọn các quân có tâm nằm trong khung. Khung hỗ trợ kéo mọi hướng, ngưỡng phân biệt click và kéo là 6 pixel. Shift cho phép chọn thêm hoặc bật/tắt từng quân. UnitSelectionInput dùng Input Actions bắt vị trí tại sự kiện bấm, di chuyển và thả để giữ đúng điểm bắt đầu kéo nhanh. Mất focus hoặc rời Game View trong Editor sẽ hủy thao tác kéo. UnitSelectionView giữ danh sách ID và cập nhật vòng xanh qua UnitView.

- Lệnh di chuyển và tìm đường hoàn thành ngày 13/11/2026, đã sửa sang tám hướng theo yêu cầu cùng ngày. Chuột phải tạo MoveCommand chứa tick thực thi, mã người chơi, số thứ tự, danh sách ID và tọa độ đích số nguyên. Offline đặt lệnh sau 2 tick. SimulationWorld chạy 20 tick/giây, xử lý lệnh theo người chơi và số thứ tự, từ chối lệnh trùng hoặc đã quá tick; khi thực thi kiểm tra quyền điều khiển và ID. GridPathfinder dùng A\* với heap, tám hướng, độ dài cạnh 1000 và chéo xấp xỉ 1414, ước lượng tương ứng cùng thứ tự phân xử theo chi phí, khoảng cách ước lượng rồi chỉ số ô. Bước chéo kiểm tra cả hai ô sát góc để ngăn xuyên góc tường. Đích ngoài map được giới hạn vào map; đích bị chặn hoặc không tới được được thay bằng ô trống trong vùng quân đi được.

Tiêu chí nghiệm thu Mốc 1 đã đạt ngày 13/11/2026: người chơi dùng chuột chọn một nhóm quân và ra lệnh cho nhóm đi trên bản đồ, vòng qua vật cản, cập nhật vòng chọn và dừng ở các vị trí riêng. Đã thử chuột thật trên bản Windows: kéo khung chọn đủ 5 quân, quan sát 5 vòng xanh, chuột phải sang phía bên kia tường và quan sát cả nhóm tới đội hình đích.

Tránh chồng quân ở Mốc 1: giữ ô hiện tại và ô sắp bước vào; khi đi chéo giữ thêm hai ô sát góc, tức vùng 2 x 2 ô, để ngăn hai đường chéo xuyên qua nhau. Ô vừa rời chỉ được quân khác dùng từ tick kế tiếp. Ô đích của từng quân được giữ riêng để tránh chặn đường vào vị trí của nhau. Khi bị chặn, quân chờ và thử lại sau mỗi 6 tick; nếu hai quân đang chờ cùng đổi đường đối xứng, quân có ID cao hơn bước sang ô trống nhường quân có ID thấp hơn rồi tiếp tục tới đích cũ. Thứ tự cập nhật xoay theo tick và ID. Đội hình ưu tiên các ô đích cách nhau 2 ô, dùng ô trống còn lại nếu thiếu chỗ. SegmentProgress lưu quãng đường trên đoạn hiện tại, chuẩn hóa bước chéo theo độ dài đoạn. Khoảng cách chưa dùng hết được giữ sang tick sau; View nội suy giữa hai trạng thái tick mà không đổi luật di chuyển.

Giới hạn của bản nền: đường đi hỗ trợ tám hướng trên lưới, đích đặt ở tâm ô. Quân đứng chiếm một ô; vùng giữ chỗ tạm thời lớn hơn khi di chuyển chéo. Bán kính sau chuẩn hóa phải từ 1 đến 499, nhỏ hơn nửa ô, và tốc độ tối đa là 20 đơn vị thế giới/giây theo cấu hình hiện tại. Vật cản Tilemap được đọc khi bắt đầu ván; chỉnh Tilemap trong lúc Play chưa tự đổi Simulation. Hành lang một ô với luồng đối đầu dài, đội hình lớn hơn và vật cản động phức tạp còn cần thử và mở rộng thuật toán. Kết nối, lobby và truyền lệnh lockstep qua mạng thuộc Mốc 8.

Kiểm chứng Mốc 1 sau sửa tám hướng ngày 13/11/2026: 42/42 test EditMode và 4/4 test PlayMode đạt, tổng 46. Bổ sung kiểm tra tốc độ ở cả tám hướng, đường chéo ngắn hơn đường gấp khúc, cấm cắt góc tường hoặc ô bị quân khác giữ chỗ, hai lệnh chéo giao nhau và đổi đích giữa đoạn chéo. Kiểm tra bán kính quân không chạm vật cản và không chồng nhau tại các mốc nội suy 0%, 25%, 50%, 75%, 100%. Bài thử 100 quân qua khe tới đủ 100 đích riêng. PlayMode xác nhận ra lệnh chéo làm đổi đồng thời X/Y trong Simulation và View. Kịch bản 100 quân và ba lệnh trong 600 tick dùng replay phiên bản 2 với Directions=8, checksum khớp ở hai lần mô phỏng và ở toàn bộ 600 tick giữa Editor với bản Windows mới. Kết quả ở Logs/Milestone1.editor-replay.txt và Logs/Milestone1.windows-replay.txt. Build Windows 64-bit thành công với 0 lỗi; đã mở bản tám hướng tại Builds/Windows/GameRTS.exe.

Điều chỉnh Mốc 1 ngày 13/11/2026: theo phản hồi của người dùng, thay di chuyển bốn hướng bằng tám hướng ngay trong Mốc 1. Phần tìm đường, tiến độ di chuyển, giữ chỗ va chạm, checksum và kiểm thử được cập nhật cùng nhau. Simulation và Commands tiếp tục dùng C# thuần và số nguyên; Unity chỉ đọc trạng thái để hiển thị. Không cần đợi Mốc 2 để có đường chéo.

Cách chạy và chỉnh map Mốc 1: mở GameScene rồi Play, hoặc chạy bản Windows cùng dữ liệu đi kèm. Chuột trái chọn hoặc kéo khung, Shift chọn thêm, chuột phải di chuyển; WASD, phím mũi tên, rê chuột sát mép và con lăn điều khiển camera. Trong Hierarchy chọn Map Grid \> Obstacles, dùng Tile Palette với RockObstacle.asset để tô hoặc xóa vật cản trước khi Play. Menu GameRTS cung cấp thiết lập scene, chạy test EditMode/PlayMode, build Windows và ghi baseline replay. Danh mục file cùng chức năng được duy trì trong role/CAU\_TRUC\_DU\_AN.md.

### Mốc 2 — Kinh tế (3–5 tuần, đang triển khai)

- Bước 1 — Ví tài nguyên và HUD hoàn thành ngày 13/11/2026: PlayerEconomyState giữ Wood, Food, Gold, Population và MaxPopulation bằng int, không dùng UnityEngine. Ví thuộc từng player trong SimulationWorld; HUD chỉ đọc. Nạp/chi nhiều loại tài nguyên kiểm tra toàn bộ trước khi thay đổi, từ chối số âm, thiếu tiền và tràn số; dân số không âm hoặc vượt sức chứa. Mỗi quân mẫu chiếm 1 dân, chỉ tăng dân khi spawn thành công.

- Ví khởi đầu: 200 Gỗ, 200 Thực, 100 Vàng, sức chứa 10. Map đang có 5 quân mẫu nên HUD hiện 5/10, không ghi cứng 3/10 trong UI. Population là số dân số sử dụng của tất cả đơn vị, không chỉ riêng loại dân lao động; chi phí dân số riêng theo từng loại quân sẽ bổ sung khi triển khai dữ liệu huấn luyện.

- Thanh HUD: EconomyHud.prefab dùng Canvas Screen Space Overlay, CanvasScaler theo màn hình tham chiếu 1280 × 720, 4 icon Thực/Gỗ/Vàng/Dân số và số đọc từ ví trong LateUpdate. Icon PNG trong suốt là hình mẫu tự tạo; có thể thay trong Prefab. Thông tin điều khiển chuyển xuống đáy màn hình. Click/chuột phải trên top bar và bảng thử không xuyên xuống chọn quân/di chuyển; camera bỏ qua zoom và edge scrolling ở các vùng này.

- Thử nạp/trừ: nút THỬ VÍ hoặc F9 mở bảng có Nạp +50 mỗi loại, Chi -50 mỗi loại, Sức chứa +5. EconomyTestInput tạo EconomyDebugCommand bất biến; World thực thi sau 2 tick và HUD hiển thị phản hồi từ kết quả thực thi. Đây là công cụ thử ví, chưa phải thu hoạch hoặc xây nhà; tăng sức chứa không sinh thêm quân. RtsGameView cấp sequence chung cho cả di chuyển và kinh tế. SimulationWorld mặc định từ chối lệnh debug, scene prototype chủ động bật Enable Economy Testing; tắt ô này để ẩn bảng thử và chặn các lệnh đó. Các giao dịch gameplay sau này phải đi qua lệnh gameplay tương ứng.

- Cấu hình trước khi Play: chọn RTS Session > RtsGameView > Starting Economy, chỉnh Starting Wood/Food/Gold/Max Population. Đổi số quân mẫu tại Unit Preview > Spawn Positions; sức chứa phải đủ số quân đặt thử, nếu vượt sẽ báo lỗi khởi tạo thay vì cho dân số vượt trần. Chỉnh giao diện bằng EconomyHud.prefab, chỉnh icon tại Art/UI. Menu GameRTS > Set up Resource HUD chỉ tạo asset còn thiếu, không ghi đè Prefab/icon đã chỉnh.

- Kiểm thử Bước 1: 52/52 EditMode và 7/7 PlayMode đạt, gồm kiểm thử Mốc 1 và 13 test mới cho ví/HUD. Đã quan sát HUD trong Play Mode, bấm nút nạp từ 200/200/100 lên 250/250/150, chi về 200/200/100 và tăng sức chứa từ 5/10 lên 5/15. Bài test xác nhận chi khi thiếu Vàng không trừ Gỗ/Thực, không trùng sequence giữa di chuyển và nạp ví, click HUD không đổi lựa chọn. Kịch bản replay nâng lên phiên bản 3, gồm 100 quân, 3 lệnh di chuyển, nạp, chi, giao dịch bị từ chối và tăng sức chứa trong 600 tick. Build Windows 64-bit thành công với 0 lỗi ngày 13/11/2026; bản headless thoát mã 0, toàn bộ file checksum 600 tick khớp byte với Editor. Kết quả tại Logs/Milestone2.editor-replay.txt và Logs/Milestone2.windows-replay.txt. Đã mở bản PC mới và quan sát đủ 4 icon, số ví ban đầu, dân số 5/10 cùng hướng dẫn. Bản thử ở Builds/Windows/GameRTS.exe. Kiểm chứng này thực hiện trên cùng máy PC, chưa thay cho kiểm tra lockstep qua mạng hoặc nhiều cấu hình máy ở Mốc 8.

- Cây, mỏ vàng, bụi thực có trữ lượng.

- Dân thu hoạch, mang về nộp, lặp vòng tự động.

- Xây công trình (đặt, xây, tiến độ).

- Tòa thị chính, nhà (giới hạn dân số), 3 xưởng.

- Hàng đợi huấn luyện dân.

Chơi được: phát triển kinh tế như đầu một ván AoE.

### Mốc 3 — Chiến đấu (4–6 tuần)

- Máu, sát thương, giáp, chết.

- Tự tìm mục tiêu, đuổi, tấn công cận chiến và tầm xa.

- Thêm lần lượt: lính giáo, cung thủ, kị binh. Sau đó pháp sư, xe công thành, lính lâu đài.

- Trại lính, chuồng ngựa và tam giác khắc chế.

- Điều kiện thắng/thua.

Chơi được: hai bên đánh nhau và phá Tòa Thị Chính.

### Mốc 4 — AI đối thủ (3–5 tuần)

- AI máy trạng thái: kinh tế → xây quân → tấn công theo đợt.

- Ba độ khó.

Chơi được: một ván hoàn chỉnh trên map 1, đánh với máy.

### Mốc 5 — Chiều sâu (4–6 tuần)

- Sương mù chiến tranh.

- Công trình phòng thủ: trạm soi, tháp cung, tường, bom.

- Thú săn, voi, quái, cá.

- Cây nâng cấp và thời đại.

### Mốc 6 — Giao diện và trải nghiệm (3–4 tuần)

- Minimap, bảng lệnh, thông báo, nhóm quân bằng phím số.

- Menu chính, chọn map/độ khó, màn hình thắng/thua.

- Âm thanh, hiệu ứng, animation.

### Mốc 7 — Bản đồ và cân bằng (3–5 tuần)

- Làm map 2–5 dựa trên hệ thống đặt tài nguyên đã có.

- Chơi thử nhiều ván, cân bằng số liệu quân, chi phí, tốc độ thu hoạch.

- Tối ưu hiệu năng.

### Mốc phát hành đầu tiên hợp lý: hết mốc 7 là một game đánh với máy hoàn chỉnh, có thể cho bạn bè chơi thử hoặc phát hành.

### Mốc 8 — Multiplayer (mục tiêu mở rộng, khoảng 6–12 tuần hoặc hơn)

- Kiểm chứng lõi deterministic và fixed tick đã xây từ các mốc trước giữa nhiều tiến trình và các máy PC, với cùng dữ liệu game và tập lệnh.

- Triển khai deterministic lockstep cho 1v1. Đồng bộ tập lệnh theo tick, kiểm soát thứ tự và lệnh trùng, xử lý dữ liệu đến muộn cùng thời gian chờ. Chọn topology, thư viện truyền mạng và cách kết nối sau khi thử nghiệm.

- Lobby, ghép trận, xử lý mất kết nối.

- So sánh checksum theo tick, ghi log desync và thử với độ trễ, jitter, mất gói, mất kết nối cùng tình huống máy mô phỏng chậm. Quy trình phục hồi hoặc kết thúc trận sẽ được xác định trong mốc multiplayer.

## 15. Việc cần quyết định sớm

1. Nền tảng phát hành đã chốt ngày 11/11/2026 là PC, điều khiển bằng chuột và bàn phím. Android xem xét sau.

2. Góc nhìn đã chốt ngày 11/11/2026. Game 2D top-down, camera cố định.

3. Kiến trúc mạng đã chốt ngày 11/11/2026 là deterministic lockstep. Các máy tự mô phỏng cùng trạng thái từ cùng seed và tập lệnh theo tick. Phần mô phỏng phải bảo đảm tính xác định từ mốc 1. Thư viện mạng, topology kết nối và dịch vụ relay còn cần lựa chọn ở mốc multiplayer.

4. Số thời đại: 2 hay 3.

5. Có làm ruộng, chợ, thuyền hay không (khuyến nghị: ruộng có, chợ và thuyền để sau).

6. Phong cách đồ họa và nguồn asset.

## 16. Checklist nhanh theo thứ tự

- Chốt nền tảng, góc nhìn, kiến trúc Simulation/View + Command

- Camera, chọn quân, di chuyển, pathfinding

- Tài nguyên, thu hoạch, xưởng, nhà, xây dựng

- Chiến đấu và 3 quân đầu

- AI cơ bản, thắng/thua, ván đánh máy đầu tiên

- Thêm 3 quân còn lại, nâng cấp, thời đại

- Sương mù, công trình phòng thủ, thú và cá

- UI hoàn chỉnh, âm thanh, hiệu ứng

- Map 2–5, cân bằng, tối ưu

- Phát hành bản đánh máy

- Multiplayer (nếu còn động lực và nguồn lực)
