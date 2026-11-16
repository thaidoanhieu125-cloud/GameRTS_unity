# Game RTS Unity — Tài liệu thiết kế & Lộ trình (làm một mình)

> Kế hoạch triển khai đang cập nhật. Bản Claude nguyên văn chỉ để đọc và đối chiếu nằm trong `TAI_LIEU_GOC_CLAUDE.txt`. Luật làm việc ở `role/LUAT_LAM_VIEC.md`; danh mục file ở `role/CAU_TRUC_DU_AN.md`.

Phong cách: Age of Empires / War of Empire Conquest · Engine: Unity · Nhân sự: 1 người

## Tiến độ hiện tại — cập nhật ngày 16/11/2026

**Mốc 0–3 và chức năng Mốc 4 Bước 1–6 đã hoàn thành. Ba mức khó/menu chọn trận đã chạy trên PC; ván kinh tế thật từ 3 Dân đến Victory được xác nhận bằng kiểm thử tự động. Cân bằng qua nhiều ván và thử thắng PvE trọn ván bằng chuột còn tiếp tục.**

| Mốc | Trạng thái | Kết quả hiện có và phần còn lại |
| --- | --- | --- |
| 0 — Chuẩn bị | Hoàn thành 11/11/2026 | Chốt PC, 2D top-down, Unity LTS, Git, Simulation/View + Command và hướng deterministic lockstep. Đã chạy Editor và build Windows. |
| 1 — Nền móng | Hoàn thành 13/11/2026 | Map 64 × 64, camera RTS, dữ liệu quân, click/kéo khung chọn, di chuyển nhóm và A* 8 hướng tránh vật cản. Đã thử 100 quân và thao tác chuột trên PC. |
| 2 — Kinh tế | Hoàn thành 6/6 bước, 14/11/2026 | Ví/HUD; nguồn hữu hạn; Dân tự thu–mang–nộp; đặt móng/xây Nhà; ba xưởng chuyên dụng; hàng huấn luyện Dân và cờ tập kết. Đã kiểm tra chu trình kinh tế trên PC. |
| 3 — Chiến đấu | Hoàn thành 6/6 bước lõi; nghiệm thu PC 16/11/2026 | HP/giáp/chết; cận chiến; tên bay; ba quân và khắc chế; ba nhà quân/huấn luyện; phá TC/thắng–thua/chơi lại. Đã thử chuột trực tiếp: xây/huấn luyện cả ba loại, hủy hoàn tiền, tập kết, giao tranh, Victory, Defeat và Chơi lại sau cả hai kết quả. Pháp sư/xe công thành/lính lâu đài còn để mở rộng. |
| 4 — AI đối thủ | Hoàn thành chức năng Bước 1–6, 16/11/2026 | Kinh tế/quân sự/tự vệ/đợt đánh, ba độ khó và UI chọn trận. 288 kiểm thử Unity/205 kiểm tra lõi đạt; Windows build0 lỗi, replay14 khớp Editor. PvE player1 từ3 Dân đến Victory kiểm chứng tự động; menu/start/back đã thử chuột trên PC. Cân bằng nhiều ván còn tiếp tục ở Mốc7. |
| 5 — Chiều sâu | Chưa hoàn thành | Đã có lõi tầm nhìn chung và ẩn đối tượng ngoài tầm thấy trong AiScene để AI chơi công bằng. Còn lớp phủ fog đen/mờ, trạm soi/tháp/tường xây được/bom, thú/cá, nâng cấp và thời đại. Vật cản Tilemap thử nghiệm chưa phải hệ công trình phòng thủ. |
| 6 — Giao diện và trải nghiệm | Chưa hoàn thành | Đã có HUD ví/xây/huấn luyện/trạng thái, bảng thắng–thua và chọn độ khó PvE. Còn minimap, nhóm quân bằng phím số, menu chính/chọn map/cài đặt, thông báo đầy đủ, âm thanh và animation. |
| 7 — Bản đồ và cân bằng | Chưa triển khai | Map hiện tại phục vụ thử kinh tế/giao tranh; còn map 2–5, cân bằng qua chơi thử và tối ưu trước phát hành. |
| 8 — Multiplayer | Chưa triển khai kết nối mạng | Đã có nền mô phỏng C# thuần, số nguyên, 20 tick/giây, Command và đối chiếu replay. Còn truyền lệnh lockstep, lobby, kết nối/mất kết nối và kiểm thử desync/độ trễ trên nhiều máy. |

### Kết quả kiểm chứng mới nhất

- Bước 5–6: **245/245 EditMode +43/43 PlayMode =288 kiểm thử Unity đạt**, cùng **205/205 kiểm tra lõi C#**. Windows64-bit build thành công, **0 lỗi**.
- Replay14 đủ **600 hàng×14 checksum**, thêm cả Easy/Medium/Hard, Editor và Windows khớp byte **145.305 byte**; headless exit0. SHA256 `1725fea578f1fd987cef666d5a652e133a60b868d5f035415a5faebca1d90f63`. Kiểm chứng cùng PC chưa thay cho mạng/nhiều cấu hình máy ở Mốc8.
- Kịch bản tự động player1 dùng kinh tế/chi phí thật, bắt đầu3 Dân rồi thu/nộp/xây/huấn luyện đủ ba binh chủng và thắng AI Dễ. Chưa ghi nhận đã chơi trọn ván PvE đến Victory bằng chuột; đây là nghiệm thu chức năng bằng kiểm thử tự động. GUI kết quả đã có kiểm thử PlayMode.
- Thử PC trực tiếp Bước5–6: thấy menu mặc định Vừa, bấm Khó/Dễ đổi mô tả đúng; chọn Dễ/Bắt đầu vào AiScene với3 Dân/ví200/200/100/3 trên10. Quay về GameScene rồi mở lại menu mặc định Vừa. Bàn giao PC tại menu chọn mức, Editor về GameScene.
- Bằng chứng tại `Logs/Difficulty.*`: core.txt, editmode.xml, playmode.xml, build.txt, editor-replay.txt, windows-replay.txt, player.log, native-menu.txt và native-player.log. Log/build bị Git bỏ qua; bản chơi tại `Builds/Windows/GameRTS.exe` cùng thư mục dữ liệu. Lịch sử các bước trước giữ ở mục14.

### Cách xem trạng thái trong tài liệu

Bảng trên là tổng quan hiện hành; mục14 giữ chi tiết từng bước và lịch sử nghiệm thu; mục16 là checklist ngắn. Các số lượng quân, chỉ số và kết quả test ở ghi chép cũ phản ánh thời điểm bước đó hoàn thành. Chỉ số chiến đấu mới nhất ở Mốc3 Bước4–6; cấu hình AI/tầm nhìn/map ở Mốc4, cập nhật đợt quân mới nhất tại Bước4. Khi công việc tiếp theo thay đổi tiến độ, cập nhật cả tổng quan, phần mốc liên quan và checklist trong chính file này.

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
| Lính giáo | Trại lính | 120 (mẫu) | 1 ô | Bền, rẻ, thắng kị ×2.5 | Chạy chậm 2 ô/giây, thua cung | 50 Thực + 25 Gỗ |
| Kị binh | Chuồng ngựa | 180 (mẫu) | 1 ô | Nhanh 6 ô/giây, áp sát cung | Thua giáo, đắt | 70 Thực + 60 Vàng |
| Cung thủ | Trường bắn | 45 (mẫu) | 6 ô | Bắn nhanh, thắng giáo từ xa | Yếu khi kị áp sát | 25 Gỗ + 45 Vàng |
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

- Thiết lập thử ở Mốc 1: GameScene có Unit Preview với UnitPreviewSpawner và RTS Session với RtsGameView, UnitSelectionView, UnitSelectionInput. Khi Play lúc đó, spawner chuyển SampleSoldierData cùng 5 Spawn Positions cho RtsGameView. Từ Mốc 2 Bước 6, cấu hình gameplay được thay bằng 3 VillagerData; lính mẫu vẫn giữ cho kiểm thử/xem độc lập. SimulationWorld cấp ID, tìm ô spawn trống gần vị trí cấu hình, sau đó View tạo Prefab tương ứng. Các vị trí cấu hình là (-4; -1,5), (-2; 1), (0; 0), (2; 1), (4; -1,5); trạng thái ban đầu được đặt ở tâm ô trống. Trong GameScene không chọn sẵn quân. Select First For Preview chỉ dùng khi xem Prefab độc lập và không gán Game cho spawner.

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

### Mốc 2 — Kinh tế (hoàn thành 6 bước ngày 14/11/2026)

- Bước 1 — Ví tài nguyên và HUD hoàn thành ngày 13/11/2026: PlayerEconomyState giữ Wood, Food, Gold, Population và MaxPopulation bằng int, không dùng UnityEngine. Ví thuộc từng player trong SimulationWorld; HUD chỉ đọc. Nạp/chi nhiều loại tài nguyên kiểm tra toàn bộ trước khi thay đổi, từ chối số âm, thiếu tiền và tràn số; dân số không âm hoặc vượt sức chứa. Mỗi quân mẫu chiếm 1 dân, chỉ tăng dân khi spawn thành công.

- Ví khởi đầu: 200 Gỗ, 200 Thực, 100 Vàng, sức chứa 10. Ở Bước 1 map có 5 quân mẫu nên HUD hiện 5/10; từ Bước 6 gameplay khởi đầu 3 Dân nên hiện 3/10. HUD luôn đọc số thực, không ghi cứng dân số. Population là số dân số sử dụng của tất cả đơn vị, không chỉ riêng loại dân lao động; Dân huấn luyện ở Bước 6 chiếm 1, chi phí dân số riêng của quân chiến đấu sẽ bổ sung cùng dữ liệu quân.

- Thanh HUD: EconomyHud.prefab dùng Canvas Screen Space Overlay, CanvasScaler theo màn hình tham chiếu 1280 × 720, 4 icon Thực/Gỗ/Vàng/Dân số và số đọc từ ví trong LateUpdate. Icon PNG trong suốt là hình mẫu tự tạo; có thể thay trong Prefab. Thông tin điều khiển chuyển xuống đáy màn hình. Click/chuột phải trên top bar và bảng thử không xuyên xuống chọn quân/di chuyển; camera bỏ qua zoom và edge scrolling ở các vùng này.

- Thử nạp/trừ: nút THỬ VÍ hoặc F9 mở bảng có Nạp +50 mỗi loại, Chi -50 mỗi loại, Sức chứa +5. EconomyTestInput tạo EconomyDebugCommand bất biến; World thực thi sau 2 tick và HUD hiển thị phản hồi từ kết quả thực thi. Đây là công cụ thử ví, chưa phải thu hoạch hoặc xây nhà; tăng sức chứa không sinh thêm quân. RtsGameView cấp sequence chung cho cả di chuyển và kinh tế. SimulationWorld mặc định từ chối lệnh debug, scene prototype chủ động bật Enable Economy Testing; tắt ô này để ẩn bảng thử và chặn các lệnh đó. Các giao dịch gameplay sau này phải đi qua lệnh gameplay tương ứng.

- Cấu hình trước khi Play: chọn RTS Session > RtsGameView > Starting Economy, chỉnh Starting Wood/Food/Gold/Max Population. Đổi số quân mẫu tại Unit Preview > Spawn Positions; sức chứa phải đủ số quân đặt thử, nếu vượt sẽ báo lỗi khởi tạo thay vì cho dân số vượt trần. Chỉnh giao diện bằng EconomyHud.prefab, chỉnh icon tại Art/UI. Menu GameRTS > Set up Resource HUD chỉ tạo asset còn thiếu, không ghi đè Prefab/icon đã chỉnh.

- Kiểm thử Bước 1: 52/52 EditMode và 7/7 PlayMode đạt, gồm kiểm thử Mốc 1 và 13 test mới cho ví/HUD. Đã quan sát HUD trong Play Mode, bấm nút nạp từ 200/200/100 lên 250/250/150, chi về 200/200/100 và tăng sức chứa từ 5/10 lên 5/15. Bài test xác nhận chi khi thiếu Vàng không trừ Gỗ/Thực, không trùng sequence giữa di chuyển và nạp ví, click HUD không đổi lựa chọn. Kịch bản replay nâng lên phiên bản 3, gồm 100 quân, 3 lệnh di chuyển, nạp, chi, giao dịch bị từ chối và tăng sức chứa trong 600 tick. Build Windows 64-bit thành công với 0 lỗi ngày 13/11/2026; bản headless thoát mã 0, toàn bộ file checksum 600 tick khớp byte với Editor. Kết quả tại Logs/Milestone2.editor-replay.txt và Logs/Milestone2.windows-replay.txt. Đã mở bản PC mới và quan sát đủ 4 icon, số ví ban đầu, dân số 5/10 cùng hướng dẫn. Bản thử ở Builds/Windows/GameRTS.exe. Kiểm chứng này thực hiện trên cùng máy PC, chưa thay cho kiểm tra lockstep qua mạng hoặc nhiều cấu hình máy ở Mốc 8.

- Bước 2 — Nguồn tài nguyên trên map hoàn thành ngày 13/11/2026: ResourceDataSO chứa ID loại, tên, ResourceType, InitialAmount và Sprite. Ba dữ liệu mẫu: TreeData (100 Gỗ), BerryBushData (150 Thực), GoldMineData (800 Vàng). Từng bản sao giữ Remaining riêng trong ResourceNodeState, không sửa trữ lượng trên asset dùng chung.

- Resource Nodes trong GameScene gồm 5 cây, 2 bụi quả, 2 mỏ vàng; mỗi nguồn chiếm 1 ô 1 × 1, sprite mẫu 64 × 64 với PPU64. RtsGameView đăng ký các ResourceNodeView đang active dưới Resource Root trong Awake trước khi spawn quân. Vị trí chuẩn hóa về tâm ô; ID nguồn = chỉ số ô + 1, không phụ thuộc thứ tự Hierarchy/đăng ký. Ngoài map, trùng nguồn, trùng ô địa hình cấm hoặc có quân sẽ bị từ chối, không tự dời nguồn sang ô khác.

- GridMap tách lớp địa hình và lớp resourceBlocked. IsWalkable chỉ đúng khi địa hình cho đi và không còn tài nguyên chặn. A* và kiểm tra góc chéo dùng chung IsWalkable nên quân vòng qua nguồn. Khi Remaining về 0, Simulation gỡ blocker tài nguyên; View tắt sprite và nhãn Stock. Trữ lượng không âm kể cả lệnh yêu cầu rút nhiều hơn số còn lại. Không mở nhầm ô địa hình cấm bên dưới. Bản ghi nguồn cạn vẫn giữ ID, không tái sử dụng; đường đã có vẫn hợp lệ, lệnh tìm đường mới sử dụng được ô vừa mở.

- Thử cạn nguồn: trỏ vào ô nguồn và bấm F8 hoặc click chuột giữa để rút thử 50; Shift+F8/Shift+chuột giữa để rút hết. ResourceTestInput tạo ResourceDebugCommand qua hàng đợi tick/sequence chung, trễ 2 tick. Cờ Enable Resource Testing trên RTS Session độc lập với thử ví; Simulation mặc định chặn lệnh thử nếu không bật. Đây chỉ là công cụ kiểm tra trữ lượng/mở ô, tài nguyên rút thử chưa cộng vào ví. Dân đi thu, mang theo và nộp tại xưởng thuộc bước sau.

- Chỉnh map: kéo Tree/BerryBush/GoldMine.prefab từ View/Prefabs/Resources vào nhóm Resource Nodes trước khi Play, đặt vào các ô trống và không đặt chồng nhau. Giữ scale của nhóm/Prefab là 1; chọn nguồn để xem gizmo footprint màu vàng. Chỉnh loại, trữ lượng và sprite trong Data/Resources. Khi Play, các bản sao dùng giá trị mới; thay đổi Transform/asset giữa ván chưa cập nhật vị trí/trữ lượng Simulation. Menu GameRTS > Set up Resource Nodes chỉ tạo asset/nhóm mẫu còn thiếu, giữ nguyên các asset và placement đã chỉnh. Nguồn chưa có nhiều giai đoạn hình ảnh khi cạn dần; hiện đổi số và ẩn khi hết.

- Kiểm tra Bước 2: 65/65 EditMode và 10/10 PlayMode đạt. Các trường hợp mới gồm chặn A*/góc chéo, rút từng phần/cạn, không ghi đè địa hình, không trừ dở dang khi placement lỗi, spawn né nguồn, ID độc lập thứ tự đăng ký, tick/sequence/checksum nguồn. PlayMode nạp scene thật với 9 nguồn, kiểm tra nhãn 100→50, ẩn khi cạn và quân đi vòng hàng cây rồi vào ô cây đã cạn. Replay nâng lên phiên bản 4, gồm 100 quân, ví và 3 nguồn với rút dần/cạn trong 600 tick. Windows 64-bit build thành công với 0 lỗi; kiểm tra headless thoát mã 0 và khớp đủ 600 tick với Editor, file tại Logs/Milestone2.resources-editor-replay.txt và Logs/Milestone2.resources-windows-replay.txt. Đã mở bản PC, quan sát đủ 3 loại nguồn với nhãn không chồng nhau, dùng chuột giữa rút cây từ 100→50→0, thấy cây biến mất và ra lệnh cho quân đứng vào đúng ô cây cũ. Đã thử chọn quân và tới phía bên kia hàng cây khi cây còn trữ lượng. Tài nguyên HUD không tăng do đây là thao tác rút thử, chưa có nộp vào ví. Phím tắt được cấu hình F8, kiểm tra thao tác thực tế trong lượt này dùng chuột giữa. Kiểm chứng Editor/Windows trên cùng máy, chưa thay thế kiểm thử mạng/nhiều cấu hình PC ở Mốc 8.

- Bước 3 — Dân và vòng lặp khai thác tự động hoàn thành ngày 13/11/2026: GatherCommand bất biến chứa Tick/PlayerId/Sequence, danh sách ID Dân và NodeId; dùng cùng hàng đợi với MoveCommand. Simulation kiểm tra toàn bộ ID, quyền sở hữu, khả năng thu và nguồn còn trữ lượng trước khi áp dụng. Chuột phải nguồn khi chọn nhóm hỗn hợp chỉ giao việc cho Dân; lính giữ lệnh hiện có. Chuột phải nền gửi MoveCommand và ngắt công việc tự thu, vẫn giữ tài nguyên đang mang.

- VillagerData là UnitDataSO có Can Gather bật, Carry Capacity = 10 và Gather Interval Ticks = 10; tốc độ 3, bán kính 0,45. Simulation chạy 20 tick/giây nên lấy 1 đơn vị mỗi 0,5 giây khi Dân đã đứng yên ở ô cạnh nguồn. Quân thường có CarryCapacity = 0/Villager = null. Unit Preview hiện dùng 3 override VillagerData ở ba vị trí đầu và 2 lính mẫu ở hai vị trí cuối, tổng dân số vẫn 5/10.

- VillagerState giữ công việc, ID nguồn/nơi nộp, sức chứa, loại/số mang cùng bộ đếm tick. Vòng lặp: GoingToResource → Gathering → GoingToDropOff → nộp toàn bộ vào đúng ví chủ sở hữu → GoingToResource. Chỉ lấy tài nguyên khi đứng ở ô sát cạnh nguồn, không lấy từ xa hoặc xuyên góc. Mỗi Dân giữ riêng ô đích làm việc; thiếu ô/chưa có đường thì WaitingForResource, thử lại sau 20 tick. Nhiều Dân dùng cơ chế đường đi/giữ chỗ của Mốc 1.

- Nơi nộp dùng DropOffState và DropOffDataSO, có chủ sở hữu, footprint và ResourceAcceptance Wood/Food/Gold/All. Tòa thị chính mẫu 2 × 2 đặt sẵn tại tâm (2,-5), ô gốc thế giới (1,-6), nhận cả 3 loại. Dân chọn công trình đúng chủ/loại gần nhất theo độ dài đường đi đến được; hòa thì phân xử theo ID công trình/ô. Dân đứng cạnh footprint để nộp, không đi xuyên công trình. GridMap bổ sung buildingBlocked độc lập với địa hình và tài nguyên. Tòa thị chính bước này chỉ là nơi nộp đặt sẵn, chưa có xây dựng tiến độ, tạo dân, máu hoặc bắn phòng thủ.

- Khi ba lô đầy, Dân ngừng lấy và tự về nộp; nguồn cạn khi chưa đầy cũng mang phần cuối về. Cập nhật ngày 14/11/2026: sau khi nộp chuyến cuối, tự tìm nguồn cùng loại trong bán kính 8 ô quanh nguồn vừa cạn, chọn nguồn đến được có đường đi ngắn nhất từ vị trí hiện tại; hòa thì ID nguồn nhỏ hơn, ô đứng làm nhỏ hơn. Không tự đổi từ Gỗ sang Thực/Vàng hoặc tự đi tìm trên toàn map. Khi đổi sang nguồn khác loại theo lệnh trong lúc đang mang hàng, Dân nộp hàng cũ trước rồi mới thu loại mới; cùng loại thì có thể mang tiếp đến nguồn mới. Thiếu nơi nhận đúng loại, đường/chỗ nộp bị chặn hoặc ví không cộng được do tràn int thì giữ nguyên ba lô, chờ và thử lại; không mất tài nguyên hoặc cộng tiền trước khi tới nơi.

- View: Villager.prefab có hình Dân áo nâu/mũ sáng, vòng chọn, nhãn Cargo 0/10 và marker ba lô đổi màu theo loại. Chọn Dân hiện Worker Status ở góc phải dưới top bar, hiển thị công việc, ba lô và trữ lượng nguồn. Panel chặn click xuyên xuống map. UnitView chỉ đọc trạng thái, không tự thu/nộp hoặc tìm đường. Sprite/animation hiện là mẫu tĩnh.

- Cách thử: vào GameScene và Play; chọn Dân áo nâu rồi chuột phải một cây/bụi quả/mỏ vàng. Quan sát 0/10 tăng dần, Dân quay về TC khi đầy, HUD tăng đúng 10 và Dân tự quay lại nguồn. Không cần click lại sau mỗi chuyến. Không dùng F8/chuột giữa khi kiểm tra kinh tế thật vì đó vẫn là công cụ rút thử bỏ tài nguyên ở Bước 2. Đổi tải trọng/nhịp trong Data/Units/VillagerData trước khi Play. Đặt thêm nơi nộp bằng TownHall.prefab dưới Drop Off Buildings, chỉnh owner/data/footprint sao cho không chồng nguồn hoặc địa hình cấm; bộ lọc SO hỗ trợ cấu hình các xưởng theo loại ở bước sau. Menu GameRTS > Set up Villager Economy tạo asset còn thiếu và nối scene, giữ dữ liệu/override đã có.

- Kiểm chứng bước Dân: lõi Commands/Simulation được biên dịch và chạy độc lập bằng Mono với 62 kiểm tra đạt; kịch bản 3 Dân thu/nộp đủ 25 mỗi loại sau 600 tick. Unity có 84/84 EditMode và 15/15 PlayMode đạt, tổng 99. Gồm bảo toàn tài nguyên, 5 Dân chia 4 ô, chuyến cuối chưa đầy, nơi nộp đúng chủ/loại/đường, chuyển nguồn, ngắt việc, tràn ví, chuột phải theo ngữ cảnh, HUD/ba lô và tính xác định. Windows 64-bit build thành công với 0 lỗi; headless thoát mã 0, khớp đủ 600 cặp checksum của replay phiên bản 5 (100 quân di chuyển + thế giới riêng 3 Dân thu/nộp). File kết quả: Logs/Milestone2.villagers-editor-replay.txt và Logs/Milestone2.villagers-windows-replay.txt. Đã thử chuột thật trên bản PC: chọn Dân 1, chuột phải cây một lần, thấy ba lô 4/10 Gỗ khi ví còn 200; sau đó ví đạt 220, Dân đã tự quay lại và mang 7/10, nguồn còn 73. Tức 100 = 20 đã nộp + 7 đang mang + 73 còn lại, không cần lệnh thứ hai. Bản chạy ở Builds/Windows/GameRTS.exe. Kiểm chứng Editor/Windows cùng máy chưa thay thế kiểm thử lockstep qua mạng/nhiều cấu hình PC ở Mốc 8.

- Bước 4 — Đặt móng và xây dựng hoàn thành ngày 13/11/2026 với lát cắt Nhà mẫu trên GameScene. Nhà chiếm 2 × 2 ô, giá 50 Gỗ/0 Thực/0 Vàng, cần 200 công, tối đa 300 HP, hoàn thiện tăng 5 sức chứa. Một Dân đã đứng cạnh móng tạo 1 công/tick; ở 20 tick/giây cần 10 giây, chưa tính thời gian đi. Nhiều Dân cùng xây cộng công theo từng Dân ở cạnh móng. Đây là số liệu prototype, có thể cân bằng trong HouseData.asset trước khi Play.

- Điều khiển: chọn ít nhất một Dân, bấm B hoặc nút XÂY NHÀ trên top bar. Ghost bám lưới, con trỏ chỉ ô góc dưới trái của footprint. Xanh khi đặt được, đỏ khi vướng địa hình/cây/công trình/quân/ô quân giữ, thiếu tài nguyên hoặc không có Dân tới được cạnh móng. Chuột trái xác nhận; chuột phải hoặc Esc hủy chế độ đặt, chưa trừ tiền. HUD thông báo lý do không đặt được. UI/gesture đặt móng không đồng thời chọn lại quân hay phát lệnh di chuyển.

- BuildingDataSO giữ ID loại, kích thước, giá, lượng công, HP, sức chứa, loại hàng nhận và sprite/Prefab. RtsGameView sao chép thành BuildingDefinition C# bất biến trước tick 1. BuildCommand chỉ chứa type ID, ô gốc, ID Dân và thông tin tick/player/sequence; không cho lệnh tự khai giá hoặc kích thước. Preview chỉ đọc CheckBuildingPlacement, không thay đổi ví/lưới/checksum. Khi tới tick thực thi sau 2 tick, Simulation kiểm tra lại toàn bộ footprint, quyền sở hữu, chi phí và đường tiếp cận; hợp lệ mới trừ đủ cả ba loại trong một giao dịch và tạo BuildingState. Lệnh trùng/đặt tranh cùng vị trí không trừ tiền lần hai.

- Móng giữ và chặn toàn bộ footprint trên lớp buildingBlocked ngay lúc đặt thành công, để quân không đi xuyên khi đang xây. Kiểm tra cả ô hiện tại, ô bước tới, hai ô cạnh đoạn chéo, đích đang giữ và ô vừa rời trong tick. Quân có đường cũ qua móng phải tìm đường lại. Hoàn thiện giữ nguyên blocker; chức năng công trình chỉ bật ở 100%. Không đợi xây xong mới chặn vì sẽ có nguy cơ quân đang đứng bên trong công trình.

- BuildingState giữ chủ sở hữu, ô gốc/footprint, công đã làm, tiến độ số nguyên và HP tăng từ 1 đến 300 theo công. Dân tự tìm ô cạnh footprint, đi tới rồi chuyển GoingToBuild → Building; chỉ đứng sát cạnh và không di chuyển mới tăng công. Thiếu đường/chỗ đứng thì WaitingToBuild, thử lại sau 20 tick. View đọc trạng thái để hiện móng, thanh tiến độ/%, búa của Dân và đổi sang hình Nhà hoàn thiện. Chuyển động búa dùng thời gian View, không tham gia Simulation.

- Ra lệnh Move hoặc Gather ngắt xây, giữ nguyên tiến độ và hàng Dân đang mang. Chọn Dân rồi chuột phải móng nhà mình chưa xong để ResumeBuildCommand tiếp tục, không trả giá lần nữa. Khi đạt đủ công, Simulation kích hoạt tăng MaxPopulation đúng một lần; 5/10 thành 5/15, không sinh thêm quân. Bộ dữ liệu cũng hỗ trợ công trình nhận hàng 3 × 3: chỉ thêm vào danh sách nơi nộp sau khi hoàn thiện, có kịch bản kiểm tra C#. Ba bộ asset/nút xây xưởng 2 × 2 được bổ sung tại Bước 5.

- Giới hạn ở thời điểm hoàn thành Bước 4: một loại Nhà xây được; Bước 5 mở rộng thêm ba xưởng, TC vẫn đặt sẵn. Chưa có phá/hủy móng, hoàn tiền, sửa chữa hoặc nhận sát thương chiến đấu; HP là trạng thái chuẩn bị cho Mốc 3. Cập nhật 14/11/2026: Dân xây xong nộp hàng đang mang rồi quay lại công việc khai thác cũ nếu còn nhớ nguồn; Dân không có việc thu trước đó và không mang hàng thì đứng chờ. Công trình không có vòng đời xóa/tái sử dụng ô; đó là phần bổ sung cùng cơ chế phá hủy. Không có mạng thực tế; giữ Simulation/Commands C# thuần, fixed tick/số nguyên để tiếp tục hướng lockstep đã chốt.

- Kiểm chứng Bước 4: Commands/Simulation biên dịch độc lập qua Mono với 84 kiểm tra lõi đạt. Unity đạt 108/108 EditMode và 21/21 PlayMode, tổng 129; thêm 22 trường hợp Simulation, 2 asset và 6 PlayMode so với Bước 3. Bao gồm preview không đổi checksum, quyền sở hữu/giá đáng tin cậy, toàn footprint 2 × 2 và 3 × 3, quân di chuyển chéo/đích giữ chỗ, tranh vị trí, chi phí nhiều loại nguyên tử, tiến độ/HP, nhiều Dân, ngắt/tiếp tục, thay đường cũ và chức năng/sức chứa chỉ bật đúng một lần sau khi hoàn thiện. Kiểm tra ngắt việc tính cả độ trễ 2 tick trước khi Move có hiệu lực.

- Windows 64-bit build thành công với 0 lỗi; bản headless thoát mã 0, file replay phiên bản 6 khớp byte đủ 600 bộ ba checksum: 100 quân di chuyển, thế giới 3 Dân thu/nộp và thế giới xây nhà/kho có ngắt/tiếp tục. Kết quả tại Logs/Milestone2.buildings-editor-replay.txt và Logs/Milestone2.buildings-windows-replay.txt. Đã mở bản PC, chọn Dân 1 và bấm nút XÂY NHÀ: quan sát bóng xanh trên đất trống, đỏ khi vướng cây, đặt nhà tại ô gốc thế giới (5,-7) làm Gỗ 200→150; Dân tự tới, hiện búa và tiến độ 56%, sau đó đổi sang hình Nhà hoàn thiện và sức chứa 5/10→5/15, Gỗ vẫn 150. Thao tác thực tế dùng nút HUD/chuột; phím B đã cấu hình, chưa xác nhận bằng bàn phím thật trong lượt này. Bản thử: Builds/Windows/GameRTS.exe. Đối chiếu Editor/Windows cùng máy chưa thay thế kiểm thử lockstep qua mạng/nhiều cấu hình PC ở Mốc 8.

- Rà soát vòng khai thác ngày 14/11/2026 theo phản hồi người dùng: lỗi khi đang giao hàng bị điều xây là AssignBuilders xóa ResourceId rồi EndBuildingJob đặt Idle. Sửa bằng giữ nguồn trong lúc xây, kết thúc việc xây thì ưu tiên nộp toàn bộ ba lô rồi StartResource; áp dụng cả Build và ResumeBuild, không cần người chơi giao lại lệnh. Người dùng đã chọn cách xây ngay, xong mới nộp và thu tiếp. Không cộng hàng trực tiếp vào ví khi giao việc xây hoặc khi hoàn thành nhà; vẫn phải đi tới nơi nộp.

- Khi nguồn cạn: ba lô còn 1–9 vẫn tự nộp trước khi tìm nguồn tiếp. Nếu nguồn cùng loại ở gần còn hàng nhưng tạm không tới được/chưa có ô làm thì SearchingForResource, thử lại mỗi 20 tick; có lối mở sẽ tiếp tục. Không còn nguồn phù hợp trong bán kính 8 ô thì nghỉ và HUD nêu rõ lý do; cần lệnh khai thác nguồn xa hơn. Lệnh Move chủ động hủy việc tự thu và nguồn ghi nhớ, giữ hàng trong ba lô; sẽ không tự kéo Dân quay lại khi người chơi vừa ra lệnh di chuyển. Lệnh Gather mới thay việc cũ, không bị công trình trước đó giành lại.

- Phản hồi về Dân bị giết: hiện chưa có HP/chết/xóa đơn vị, nên chưa có trường hợp chết trong bản chơi để kiểm tra. Quy tắc dự kiến cho Mốc 3 là hàng chưa nộp mất theo Dân, ví chỉ giữ số đã nộp; không tự chuyển ba lô vào ví lúc chết. Khi triển khai phải kiểm tra giải phóng ô/đích giữ chỗ, giảm dân số đúng một lần, hủy việc và từ chối lệnh của ID đã chết. Đây là kế hoạch, chưa phải tính năng đã triển khai trong lần sửa kinh tế này.

- Kiểm tra bản sửa ngày 14/11/2026: 98 kiểm tra lõi C# độc lập đạt; Unity EditMode 122/122 và PlayMode 23/23 đạt, tổng 145. Có 14 trường hợp mới cho chuyến cuối Gỗ/Thực/Vàng, xây khi đang nộp 3 hoặc 10, tiếp tục việc cũ, thiếu nơi nộp, Move hủy ghi nhớ, nguồn khác loại/quá xa/không có đường, mở lại đường rồi retry, phân xử hòa theo ID, nhiều Dân chia phần cuối và bảo toàn tài nguyên từng tick. Hai PlayMode mới nạp GameScene thật, kiểm tra đặt nhà khi đang mang 10 rồi nộp/thu tiếp, cùng nguồn cạn nộp 3 rồi đổi cây/HUD. Các kiểm tra cũ tiếp tục đạt. Replay phiên bản 7 thêm thế giới phục hồi việc Dân, ghi bốn checksum mỗi tick.

- Bản sửa 14/11 đã build Windows 64-bit thành công, exit 0 và 0 lỗi. Headless exit 0, file 600 bộ bốn checksum khớp byte với Editor: Logs/Recovery.editor-replay.txt và Logs/Recovery.windows-replay.txt. Kết quả test ở Logs/Recovery.editmode.xml và Logs/Recovery.playmode.xml; build ở Logs/Recovery.build.txt. Đã thử chuột trên PC: ví 220, Dân mang 9/10, đặt nhà làm ví 170; trong lúc xây 42% vẫn giữ đủ 9. Nhà hoàn thiện 5/15, Dân tự nộp để ví lên 179 rồi quay lại cây và mang tiếp 7/10, nguồn còn 64. Đã dùng công cụ rút thử -50 để đẩy nhanh việc cạn cây; sau khi cây cũ hết, Dân tự sang cây bên cạnh từ 100 còn 98 với ba lô 2/10 mà không nhận Gather mới. Rút thử không cộng vào ví; bảo toàn thu hoạch thật được kiểm tra bằng các test trữ lượng nhỏ. Bản PC ở Builds/Windows/GameRTS.exe; kiểm chứng cùng máy chưa thay thế kiểm thử mạng/nhiều cấu hình PC.

- Khắc phục Play màn hình trống ngày 14/11/2026: Editor đang mở Untitled chỉ có Main Camera, không có map/quân/HUD. Đã mở lại Assets/Game/Scenes/GameScene.unity, kiểm tra trực tiếp Play hiện đủ map, 3 Dân + 2 lính, 9 nguồn, TC và ví 200/200/100, dân số 5/10. Bổ sung menu GameRTS > Open GameScene (action game-scene): lưu bản sao các scene đang mở vào Logs/SceneBackups trước khi chuyển, dừng nếu lưu không thành công và từ chối khi đang Play. Giữ bản backup Untitled sẵn có của Unity trong cùng nhóm log. File yêu cầu kiểm tra bị khóa tạm thời sẽ thử lại thay vì tạo IOException mỗi frame. Không thay đổi Simulation hoặc bản PC; kiểm chứng lần này bằng biên dịch Editor và Play trực tiếp. Sau các lượt kiểm thử về sau phải đưa Editor về GameScene trước khi bàn giao.

- Bước 5 — Ba xưởng nhận tài nguyên chuyên dụng hoàn thành ngày 14/11/2026: Xưởng Gỗ (lumber_camp) chỉ Wood, Xưởng Thực (mill) chỉ Food, Xưởng Vàng (mining_camp) chỉ Gold. Cả ba 2 × 2 ô, giá 100 Gỗ/0 Thực/0 Vàng, cần 200 công (10 giây với một Dân đứng xây), 400 HP và PopulationBonus = 0. Công/HP là số liệu mẫu có thể chỉnh trước khi Play trong các asset Data/Buildings. Tòa thị chính đặt sẵn vẫn nhận All; xưởng chưa đặt sẵn trên map, cần xây trong ván.

- Dùng chung hệ đặt móng/BuildCommand/ResumeBuildCommand: bóng xanh/đỏ kiểm tra cả footprint và ví; xác nhận mới trừ 100 Gỗ đúng một lần. Móng chặn đường ngay lúc đặt. Chỉ khi đủ công mới thêm DropOffState vào danh sách nơi nhận của chủ sở hữu. Dân đầy ba lô (hoặc nguồn cạn chưa đầy) chọn công trình đã hoàn thiện, đúng chủ và đúng loại hàng theo tổng độ dài đường A* tới ô đứng nộp, không đo đường thẳng xuyên vật cản. Không đến được thì bỏ qua, hòa theo ID công trình/ô. Chọn lại nơi nộp khi bắt đầu mỗi chuyến; xưởng xây xong giữa chuyến không buộc Dân đổi ngay đường đang đi. Dân đang chờ nơi nộp thử lại mỗi 20 tick nên tự dùng xưởng mới khi tới được.

- UI xây: chọn ít nhất một Dân hiện bốn nút Nhà / Xưởng Gỗ / Xưởng Thực / Xưởng Vàng thành hàng bên trái dưới thanh ví; mỗi nút ghi giá. Chọn lính hoặc bỏ chọn sẽ ẩn hàng nút. Phím B vẫn chọn Nhà. Chọn loại khác khi đang có ghost sẽ chuyển bóng sang loại đó; bấm lại cùng loại hoặc phải/Esc thì hủy. Notice đặt móng nằm dưới hàng nút, Worker Status ghi đúng tên công trình đang xây. Các nút/notice chặn click chọn/di chuyển và camera cuộn xuyên xuống map.

- Tạo asset: DropOffCampTools (menu GameRTS > Set up Drop-off Camps) tạo dữ liệu, Prefab và sprite còn thiếu, nối catalog 4 loại trong GameScene và bốn nút HUD; giữ asset đã có và từ chối setup khi đang Play/scene chưa lưu. Ba hình mẫu 128 × 128, PPU64, dùng chung móng Nhà nhưng hình hoàn thiện phân biệt bằng mái xanh lá/chồng gỗ, mái đỏ/bao thực và mái xám/đá vàng. View chỉ hiển thị; Simulation vẫn C# thuần và chỉ giữ ID loại/bộ lọc, không phụ thuộc Sprite/GameObject.

- Cách thử Bước 5: mở GameScene và Play, chọn Dân, chọn nút xưởng, click đất trống cạnh nguồn, đợi hoàn thiện rồi chuột phải nguồn để giao khai thác. Xưởng gần và đúng loại sẽ được chọn thay TC; xưởng sai loại hoặc còn là móng bị bỏ qua. Ví đầu có 200 Gỗ, đủ hai xưởng; muốn xây cả ba cần thu thêm ít nhất 100 Gỗ. Bảng thử ví F9 vẫn là công cụ debug, không phải kinh tế thật. Bước này chưa làm nâng cấp tốc độ khai thác, huấn luyện Dân hoặc phá hủy xưởng.

- Kiểm tra Bước 5: lõi Commands/Simulation biên dịch độc lập và đạt 108 kiểm tra. Unity đạt 136/136 EditMode, 27/27 PlayMode, tổng 163; thêm 10 kiểm tra Simulation, 4 dữ liệu/asset và 4 PlayMode. Test chạy cả ba nút trong GameScene, xác nhận chi đúng 100 Gỗ, không tăng sức chứa, đổi hình hoàn thiện và Dân nộp vào đúng xưởng/đúng ví. Bao gồm sai loại về TC, móng bị bỏ qua, dùng xưởng ở chuyến tiếp, Dân chờ nơi nộp tự dùng xưởng mới, đo sản lượng với đường ngắn và bảo toàn tài nguyên + chi phí xây từng tick. Replay phiên bản 8 có năm thế giới; thế giới xưởng xây cả ba, thu/nộp đủ 25 mỗi loại trong 600 tick.

- Nghiệm thu Bước 5: Windows 64-bit build thành công, 0 lỗi; headless exit 0 và file đủ 600 bộ năm checksum khớp byte Editor/Windows. Kết quả: Logs/Camps.editmode.xml, Camps.playmode.xml, Camps.build.txt, Camps.editor-replay.txt và Camps.windows-replay.txt. Đã mở bản PC, quan sát hàng nút ẩn khi chưa chọn và hiện đủ bốn nút khi chọn Dân; bấm Xưởng Gỗ, đặt cạnh hàng cây làm ví 200→100, thấy tiến độ/tên xưởng và hình mái xanh có gỗ khi xong. Ra một Gather, Dân tự thu/nộp ba chuyến, ví lên 130, ba lô 6 và cây còn 64: 100 = 30 đã nộp + 6 đang mang + 64 còn lại. Không dùng công cụ nạp ví/rút nguồn trong lần thử này. Editor đã đưa về GameScene sau kiểm tra; bản PC tại Builds/Windows/GameRTS.exe. Kiểm chứng cùng máy chưa thay cho thử lockstep qua mạng/nhiều cấu hình PC ở Mốc 8.

- Hoàn thành Bước 6 ngày 14/11/2026 — hàng đợi huấn luyện Dân và điểm tập kết. Ván trong GameScene bắt đầu với 1 Tòa thị chính (TC) và đúng 3 Dân, 200 Gỗ / 200 Thực / 100 Vàng, dân số 3/10. Hai lính thử của các bước trước không còn spawn trong ván này; asset lính và spawner độc lập vẫn giữ để kiểm thử.

- Chọn TC bằng chuột trái lên footprint 2 × 2; vòng chọn xanh và bảng Training ở góc trái dưới hiện ra. Bảng có nút Huấn luyện Dân (50 Thực · 10 giây), Queue [x/5], số chỗ đã đặt trên mọi TC của người chơi, vòng tiến độ và 5 icon. Chọn/quét quân hoặc click đất trống bỏ chọn TC. UI chỉ hiển thị; nút → TrainingInput → TrainingCommand dùng Tick/PlayerId/Sequence chung với các lệnh khác. Command xử lý sau 2 tick như Move/Build, trừ tiền khi nhận vào hàng đợi, không chờ sinh xong mới trừ.

- Simulation kiểm tra đúng chủ TC, queue chưa đủ 5, Food ≥ 50 và Population + tổng queue mọi TC < MaxPopulation. Không đạt thì không trừ tiền, không thêm lượt. Thiếu chỗ báo đúng “Cần xây thêm Nhà dân”; xây xong Nhà vẫn tăng giới hạn +5 theo Bước 4. Mỗi lượt giữ 1 chỗ nhưng chưa tăng Population trên HUD. Hàng đợi chứa cả lượt đang huấn luyện; chỉ lượt đầu tăng công, đủ 200 tick ở 20 tick/giây thì sinh 1 Dân. Ba lượt nối nhau cần khoảng 30 giây.

- Chuột phải một icon gửi ID lượt ổn định, hủy chính lượt đó và hoàn đúng giá đã trả (50 Thực theo asset). Hủy lượt đầu thì lượt kế bắt đầu từ 0; hủy lượt sau không mất tiến độ lượt đầu. Hủy lại/ID vừa hoàn thành không được hoàn hai lần và không hủy nhầm lượt mới dồn lên. Trường hợp ví chạm trần int không thể nhận tiền hoàn: giữ lượt, báo rõ lý do, không xóa mất tiền/lượt.

- Sinh quân là thay đổi trong Simulation ở cuối tick: chọn ô đi được sát cạnh footprint TC, không có quân/ô đang giữ/đích quân khác/ô vừa rời trong tick này. Nếu cửa ra bị bịt kín thì giữ lượt ở 100%, hiển thị chờ ô trống, không mất chi phí và chưa tăng dân số. Có chỗ sẽ tự sinh ở tick tiếp theo; vẫn có thể hủy hoàn tiền. ID đơn vị nối tiếp ổn định; RtsGameView thấy trạng thái mới thì tạo UnitView từ VillagerData đã đăng ký, vòng chọn/ba lô/di chuyển dùng lại hệ thống hiện có.

- Chọn TC rồi chuột phải map gửi SetRally, ghi ô cờ trong trạng thái mô phỏng. Cờ chỉ hiện khi đang chọn TC. Đặt trên đất thì Dân mới tự nhận Move; đặt trên cây/bụi quả/mỏ còn trữ lượng thì tự nhận việc Gather, mang đủ 10 về nộp tại nơi đúng loại/gần nhất và lặp lại. Nếu nguồn đã cạn trong lúc đang huấn luyện, dùng lại cơ chế tìm nguồn cùng loại trong bán kính 8 ô. Đổi cờ áp dụng cho Dân sinh sau đó; không tự đổi việc của Dân đang có. Chuột phải trên icon/UI không bị hiểu thành lệnh cắm cờ xuyên xuống map.

- Cấu hình: Assets/Game/Data/Buildings/VillagerTrainingData.asset giữ UnitData = VillagerData, Food Cost = 50, Duration Ticks = 200, Queue Limit = 5; số giây = tick / 20. TrainingDefinition sao chép chỉ số sang C# thuần trước tick 1, không cho lệnh gửi giá/tốc độ tùy ý. TrainingState/TrainingTicket, mã ID tiếp theo, cờ, giá, tiến độ, dữ liệu Dân và lệnh chờ đều nằm trong checksum. Commands/Simulation tiếp tục không tham chiếu UnityEngine. TrainingTools (GameRTS > Set up Villager Training) tạo asset/ảnh/HUD còn thiếu, nối scene và đặt 3 Dân đầu; không cần chạy lại setup để Play scene đã lưu.

- Kiểm tra Bước 6: lõi Commands/Simulation biên dịch độc lập đạt 124/124; Unity đạt 154/154 EditMode + 31/31 PlayMode, tổng 185. Bao gồm trừ ví, đúng 200 tick, 5 lượt tuần tự, dân đặt chỗ nhiều TC, hủy đầu/cuối/ID cũ, cửa ra kín/chiếm, hoàn tràn int, cờ đất/nguồn và input/UI/UnitView/HUD trong scene thật. TrainingScenario chạy 1200 tick: 3 Dân thu Gỗ/Thực, xây Xưởng Vàng 100 Gỗ/200 công, nộp vàng tại xưởng, huấn luyện đủ 3 Dân mới tự khai thác; mỗi tick kiểm tra tổng tài nguyên còn trong nguồn + ba lô + ví + chi phí xây/huấn luyện bảo toàn và dân số không vượt sức chứa.

- Windows 64-bit build thành công, 0 lỗi. Replay phiên bản 9 thêm thế giới kinh tế/huấn luyện vào 5 thế giới cũ: 600 bộ sáu checksum Editor/PC khớp byte (63640 byte), player exit 0. Kết quả ở Logs/Training.editmode.xml, Training.playmode.xml, Training.build.txt, Training.editor-replay.txt, Training.windows-replay.txt. Đây là đối chiếu Editor/Windows trên cùng máy; kiểm chứng nhiều máy và truyền lệnh lockstep thực tế vẫn thuộc Mốc 8.

- Cách thử: mở GameScene, Play; chọn TC, chuột phải một cây làm cờ rồi bấm huấn luyện 2–3 lần. Thấy Thực giảm 50 mỗi lượt, vòng tiến độ quay, dân số lần lượt 3 → 4 → 5 → 6 và Dân mới tự đi làm. Chuột phải icon phía sau để hủy và xem +50 Thực. Chọn một Dân khác xây Xưởng Vàng cạnh mỏ (100 Gỗ), đợi xong rồi chuột phải mỏ; Dân nộp tại xưởng. Không cần bảng debug để vận hành kinh tế.

- Nghiệm thu trực tiếp bản PC: thấy ban đầu 3/10, chọn TC hiện bảng/vòng chọn; đặt cờ lên cây, bấm hai lượt làm Thực 200→100, hủy icon thứ hai hoàn về 150. Dân đầu tự sinh (4/10), tới cây và nộp gỗ nhiều chuyến (ví lên 210 rồi 230). Bấm thêm hai lượt làm Thực còn 50; quan sát đủ 6/10 và các Dân tự thu/nộp. Đã chọn Dân/nút Xưởng Vàng, thấy ghost hợp lệ, đặt móng sát mỏ và tiến độ xây; khi hoàn thiện, quan sát dân thu/nộp tại vùng xưởng và ví Vàng tăng từ 100 lên 220 trong lần thử tiếp tục. Không dùng nút thử ví/rút nguồn trong phần nghiệm thu này. Giữ cửa sổ PC đang chạy để người dùng thử tiếp; Editor được đưa về GameScene. Tiêu chí sáu bước Mốc 2 đã đạt: ví → nguồn hữu hạn → thu/nộp → xây → xưởng đúng loại → huấn luyện/tập kết tự động.

- Giới hạn khi hoàn thành Mốc 2 ngày 14/11/2026, trước phần chiến đấu: TC đặt sẵn, chưa xây TC mới trong UI; Simulation hỗ trợ đăng ký nhiều TC lúc khởi tạo và đã kiểm tra dân đặt chỗ chung. Khi đó chưa có chiến đấu/chết/phá TC hoặc hàng chờ; các phần này đã được bổ sung ở Mốc 3. Rally theo đối tượng di động, nâng cấp thời gian huấn luyện và animation sinh quân vẫn chưa có. Hủy icon chỉ áp dụng cho hàng huấn luyện, chưa phải hủy móng. Đồ họa vẫn là mẫu kỹ thuật.

Chơi được: phát triển kinh tế như đầu một ván AoE.

### Mốc 3 — Chiến đấu (hoàn thành 6 bước lõi ngày 15/11/2026; nghiệm thu PC ngày 16/11/2026)

Trạng thái sáu bước được yêu cầu: đã triển khai, kiểm chứng bằng kiểm thử tự động và thử chuột trực tiếp trên bản PC sau Bước 4–6. Lượt nghiệm thu PC kết thúc ngày 16/11/2026, xác nhận cả hai màn thắng/thua và Chơi lại. Phạm vi này dùng ba quân cơ bản; ba quân mở rộng trong thiết kế gốc chưa triển khai.

- [x] Bước 1 — MaxHP/CurrentHP/Armor, thanh máu, chết giải phóng ô và giảm dân số.
- [x] Bước 2 — AttackCommand, áp sát/đuổi, sát thương tối thiểu 1 và trả đòn tự động.
- [x] Bước 3 — Bắn xa, mũi tên theo tick, chạm mục tiêu mới trừ HP.
- [x] Bước 4 — Giáo/cung/kị và vòng khắc chế; giáo đánh kị nhân 2,5 trước giáp.
- [x] Bước 5 — Trại lính/Trường bắn/Chuồng ngựa, giá quân, hàng đợi/hủy hoàn tiền và cờ tập kết.
- [x] Bước 6 — Phá TC chốt thắng/thua/hòa, khóa ván và nút Chơi lại trong CombatScene.
- [x] Chơi thử bằng chuột trên bản PC sau Bước 4–6: xây/huấn luyện ba loại, hủy/tập kết, giao tranh, Victory/Defeat và Chơi lại — đạt ngày 16/11/2026.

Ghi chép chi tiết bên dưới đi theo thứ tự Bước 1–3 rồi Bước 4–6; cấu hình ở phần sau thay thế số liệu mẫu tương ứng ở phần trước.

- Phạm vi Bước 1–3 hoàn thành ngày 14/11/2026: chỉ số/máu/chết, cận chiến/AttackCommand/dò và trả đòn, tấn công xa/đạn theo tick. GameScene vẫn là map kinh tế 3 Dân; CombatScene dùng chung map nền và Prefab/SO/lõi mô phỏng để thử hai phe. Nút THỬ CHIẾN ĐẤU ở góc phải dưới nạp CombatScene; VỀ BẢN ĐỒ KINH TẾ quay lại GameScene. Mỗi lần chuyển là một ván mới, không giữ tiến độ của map trước. Tại thời điểm đó chưa có kị binh, khắc chế, nhà quân và thắng/thua; các phần này đã được bổ sung ngày 15/11/2026 ở Bước 4–6 bên dưới.

- Bước 1 — UnitDataSO có MaxHP, Armor, AttackDamage, AttackIntervalTicks, AttackRangeCells, ProjectileSpeed và AggroRangeCells; tạo CombatStats bất biến trước tick 1. UnitState giữ CurrentHP/IsAlive, mục tiêu, hồi chiêu và các tick đánh/nhận đòn. HP là số nguyên. HealthBarView trên Dân/lính/TC/Nhà/xưởng chỉ đọc tỷ lệ và nhãn máu, xanh cho player1, đỏ cho địch. UnitView có chớp đòn đánh ngắn; đây vẫn là hình mẫu, chưa phải animation tấn công nhiều hướng.

- Khi HP về 0: bỏ toàn bộ giữ chỗ ô hiện tại/ô kế/hai ô góc chéo/ô đích, ngừng di chuyển/khai thác/xây/đánh và giảm Population đúng 1. Không đổi lớp địa hình/cây/công trình. Ba lô chưa nộp mất theo Dân, không cộng vào ví; đây là hiện thực quy tắc đã chốt khi rà soát kinh tế. View ẩn quân/vòng chọn và selection bỏ ID chết. Giữ bản ghi chết trong danh sách Units để không tái dùng ID; AliveUnitCount mới đếm quân sống. Lệnh nhóm đã xếp lịch bỏ qua thành viên chết và vẫn áp dụng cho thành viên còn sống; ID không tồn tại/sai chủ vẫn làm lệnh bị từ chối.

- Bước 2 — Chuột phải trúng quân/footprint nhà địch khi chọn quân gửi AttackCommand chứa TargetKind Unit/Building, TargetId và danh sách ID quân bất biến, dùng tick/player/sequence chung và độ trễ offline 2 tick. Simulation xác thực đúng chủ, còn sống, có công và mục tiêu khác phe; lệnh không mang giá trị sát thương tùy ý. Click trái/quét chỉ chọn quân mình còn sống. Move/Gather/Build/ResumeBuild chủ động ngắt việc đánh; Attack ngắt việc kinh tế, giữ hàng đang mang nhưng không tự quay lại khai thác sau giao tranh. Hồi chiêu không bị reset bằng cách spam lệnh.

- Cận chiến tầm 1 ô kề ở cả 8 hướng, không đánh xuyên góc tường bị chặn. Chỉ đánh khi đã đứng yên; nếu xa thì chọn ô tấn công trống/đến được và đi bằng A* 8 hướng, giữ ô riêng như di chuyển. Kiểm tra/chỉnh đường đuổi mỗi 10 tick để theo mục tiêu di động, không ghi đè lệnh di chuyển mỗi frame. Sát thương mỗi lần = max(1, AttackDamage - Armor). Lính đang rảnh tự chọn địch gần trong AggroRange; hòa ổn định theo loại mục tiêu và ID. Dân có AggroRange = 0 để không tự bỏ kinh tế đi tìm địch. Đơn vị rảnh bị đánh sẽ trả đòn nếu có công và người đánh còn sống; Dân đang làm việc hoặc quân đang nhận lệnh Move không bị tự đổi việc vì một đòn trúng.

- Bước 3 — Cung thủ mẫu có tầm 5 ô, khoảng cách tròn tính bình phương số nguyên; tới trong tầm thì dừng bắn từ xa. Mỗi phát tạo ProjectileState có ID riêng, nguồn/phe/mục tiêu/công/tốc độ/tick phóng và vị trí trước/hiện tại. Từ tick sau phát bắn, tên truy đuổi vị trí thật của mục tiêu bằng bước số nguyên có phần dư; chỉ khi tới điểm đích mới tính max(1,công-giáp) và trừ HP. Không dùng Unity Physics, Rigidbody, Collider hoặc thời gian animation để tính trúng. Người bắn chết thì tên đã phóng vẫn bay; mục tiêu chết thì tên biến mất, không trúng nhầm quân/nhà mới. ProjectileView chỉ nội suy/xoay bằng float; RtsGameView tái dùng hình tên qua pool.

- Phá công trình: BuildingState có giáp/IsDestroyed, TC đặt sẵn có HP/giáp trong DropOffState. Xưởng hoàn thiện liên kết HP với BuildingState, không có hai ví máu độc lập. Hết HP ẩn hình và mở footprint; xóa nơi nộp hoạt động để Dân chọn lại/chờ nơi khác; TC mất luôn queue và các chỗ đặt, không hoàn chi phí huấn luyện bị phá. Nhà hoàn thiện bị phá trừ PopulationBonus khỏi MaxPopulation; dân hiện có không bị xóa nếu vượt giới hạn mới, lượt sinh chờ đến khi có chỗ. Xây lại trên đất cũ dùng ID mới để lệnh/tên cũ không tác động vào nhà mới. Khi đang xây, công mới chỉ cộng lượng HP tương ứng với công đó, không ghi đè sát thương đã chịu về mức HP theo tiến độ.

- Số liệu thử: Dân 40 HP, giáp 0, công 3, chu kỳ 30 tick, tầm 1, không tự dò. Lính giáo mẫu (MeleeData): 120 HP, giáp 2, công 12, chu kỳ 20 tick, tầm 1. Cung thủ (ArcherData): 70 HP, giáp 0, công 10, chu kỳ 24 tick, tầm 5, tên 8000 đơn vị/giây (8 ô/giây). Cả hai lính tốc độ 3, tự dò 6 ô, chiếm 1 dân. TC mẫu 1000 HP/giáp 2; Nhà/xưởng giữ HP của Mốc 2 và giáp 0 mặc định. Đây là chỉ số prototype, chỉnh asset Data/Units hoặc Data/Buildings trước khi Play; chưa phải cân bằng hoặc tam giác khắc chế cuối cùng.

- CombatScene khởi đầu 3 Dân + lính giáo/cung thủ của player1 (5/10), lính giáo/cung thủ của player2, TC địch ở (12,-5). Camera ban đầu tâm (4,0); hai phe ở hai phía tường để thử tìm đường. UnitPreviewSpawner tạo 3 Dân trước, CombatPreviewSpawner tạo 4 lính sau; RtsGameView đăng ký player2 trước khi nạp nhà và cấp ID/trạng thái bằng cùng World. Quân địch tự dò/trả đòn theo CombatStats; chưa có AI xây dựng/kinh tế, chiến thuật đội hình hoặc tăng viện.

- Kiểm tra: lõi C# Commands/Simulation biên dịch và chạy độc lập qua Mono đạt 143/143. Unity đạt 176/176 EditMode + 36/36 PlayMode, tổng 212. Thêm 19 trường hợp Simulation, 3 asset và 5 PlayMode: sát thương/giáp tối thiểu/cooldown, đuổi mục tiêu di động/vòng tường, cấm xuyên góc, phát hiện hòa, trả đòn nhưng giữ việc Dân bận, tên chỉ trừ máu lúc tới/đuổi mục tiêu/người bắn chết, tên mất khi mục tiêu chết, giải phóng cả giữ chỗ chéo/đích, mất ba lô/giảm dân, không tái dùng ID, nhóm có thành viên chết, phá TC/queue/footprint, nhà mất sức chứa/xây lại ID mới, xây không hồi sát thương đã mất. PlayMode kiểm tra input vào quân/nhà địch, tên/HealthBar, Dân chết bỏ chọn/HUD và nút chuyển map thật. Đã sửa lỗi thứ tự khởi tạo player2 và tham chiếu SO sau khi nhập/lưu asset được PlayMode phát hiện; bộ kiểm tra cũ tiếp tục đạt.

- Build/đối chiếu Bước 1–3: Windows 64-bit thành công ngày 14/11/2026, 0 lỗi; chứa cả GameScene và CombatScene, mặc định mở GameScene. Replay phiên bản 10 ghi 600 bộ bảy checksum (di chuyển/thu-nộp/xây/phục hồi/xưởng/huấn luyện/giao tranh). Headless player exit 0, hai file Editor/Windows khớp byte đủ 73850 byte. Kết quả ở Logs/Combat.editmode.xml, Combat.playmode.xml, Combat.build.txt, Combat.editor-replay.txt, Combat.windows-replay.txt và Combat.player.log; bản chơi ở Builds/Windows/GameRTS.exe cùng thư mục dữ liệu.

- Nghiệm thu bằng chuột trên PC: thấy map kinh tế 3/10 với HealthBar và nút chuyển map; bấm THỬ CHIẾN ĐẤU chuyển sang map 5/10 có quân hai phe/thanh xanh đỏ/TC địch. Chọn cung thủ mình, phải vào lính địch: cung thủ đi tới vị trí bắn bên trái tường, địch đuổi, quân còn lại tự phát hiện/tham chiến; quan sát thanh HP thay đổi, quân chết biến mất, lựa chọn còn 0 và dân số mình 5→4. Đường bay/nhịp trừ HP/hiện ProjectileView được xác nhận chi tiết bằng PlayMode, không chỉ dựa vào ảnh chụp trong trận. Giữ bản PC mở để người dùng thử tiếp. Editor bàn giao ở GameScene; muốn chơi lại map chiến đấu bấm nút trong game hoặc mở Assets/Game/Scenes/CombatScene.unity rồi Play.

- Giới hạn tại thời điểm hoàn thành Bước 1–3 (trước phần bổ sung 15/11): tên hiện truy đuổi mục tiêu và bay qua địa hình/công trình trung gian, không có né tên, va chạm dọc đường hoặc sát thương lan. Đang dùng một chỉ số giáp chung, chưa chia sát thương cận/xa/phép/công thành. Dò/đuổi chưa có bán kính rút về vị trí canh gác, tầm nhìn/fog, stance hoặc lệnh Stop riêng. Chưa có xác chết, âm thanh, animation chuẩn, sửa nhà, thưởng kinh nghiệm hoặc màn thắng/thua. Truyền lệnh multiplayer/multi-PC thuộc Mốc 8. Không dùng kết quả đối chiếu Editor/Windows cùng máy để khẳng định đã có mạng lockstep hoàn chỉnh.

- Bước 4–6 triển khai ngày 15/11/2026 theo yêu cầu mới: hoàn thiện bộ ba giáo/cung/kị, nhà quân/huấn luyện và điều kiện phá TC để kết thúc ván. Giữ GameScene cho kinh tế độc lập; CombatScene dùng đầy đủ kinh tế + quân sự + TC hai phe + thắng/thua. Bộ ba là phạm vi chiến đấu đầu tiên; pháp sư/xe công thành/lính lâu đài tiếp tục là nội dung mở rộng, không tự sinh thêm trong bước này.

- Khắc chế: CombatStats có UnitClass Generic/Villager/Spearman/Archer/Cavalry, không suy loại quân từ tên tiếng Việt hay Prefab. Giáo đánh Cavalry tính max(1, floor(Công × 5/2) − Giáp); nhân trước khi trừ giáp, dùng long trung gian rồi về int, không dùng float. Ví dụ công 12 đánh kị giáp1 gây 29, đánh quân khác giáp1 vẫn 11. Không cộng ×2.5 khi đánh nhà. ProjectileState ghi lớp người bắn lúc phóng để đạn đã bay không phụ thuộc người bắn còn sống. Lớp quân và cấu hình có trong checksum.

- Cấu hình mẫu đã điều chỉnh để vòng khắc chế thể hiện trong mô phỏng: giáo 120 HP/giáp2/công12/20 tick/tầm1/tốc độ2; cung 45 HP/giáp0/công18/12 tick/tầm6/tốc độ3/tên10000; kị 180 HP/giáp1/công16/20 tick/tầm1/tốc độ6. Lính cùng dò địch trong 6 ô, chiếm 1 dân. Chỉ giáo-vs-kị có hệ số thưởng hiện tại; cung-vs-giáo và kị-vs-cung dựa vào tầm/nhịp bắn/máu/tốc độ. Ba trận kiểm tra riêng cho kết quả đúng vòng: giáo thắng kị ở cận chiến; cung thắng giáo từ cự ly 6 ô; kị thắng cung từ cùng cự ly. Không khẳng định mọi đội hình/địa hình/số lượng đều giữ kết quả này; cần cân bằng qua chơi thử. MeleeData.asset giữ tên file/ID melee để không đứt tham chiếu, Class = Spearman; ArcherData và CavalryData có Class tương ứng.

- Ba nhà quân: Trại lính (barracks), Trường bắn (archery_range), Chuồng ngựa (stable); đều footprint2 × 2, giá150 Gỗ, 200 công/10 giây một Dân đã đứng cạnh, 600 HP/giáp2, không tăng dân số và không nhận nộp tài nguyên. Dùng chung BuildCommand/ghost/đường Dân/tiến độ/HP/kiểm tra đặt móng; chỉ khi xây100% mới đăng ký hàng huấn luyện. Bảy nút xây khi chọn Dân được bố trí hai hàng: Nhà/ba xưởng ở trên, ba nhà quân ở dưới; notice chuyển xuống dưới cả hai hàng.

- Chi phí quân đúng yêu cầu: Trại lính → giáo50 Thực +25 Gỗ; Trường bắn → cung25 Gỗ +45 Vàng; Chuồng ngựa → kị70 Thực +60 Vàng. Mỗi loại 200 tick/10 giây, tối đa5 lượt gồm lượt đang huấn luyện, dùng chung hệ giữ chỗ Population + tổng queue của mọi nhà/TC < MaxPopulation. Giá/thời gian/loại quân nằm trong TrainingDataSO được BuildingDataSO tham chiếu; CreateDefinition sao chép toàn bộ dữ liệu sang C# thuần trước tick1. Không gửi giá do người chơi tự khai qua Command.

- Hàng huấn luyện dùng chung: TrainingState có footprint riêng để nhà quân không cần giả thành depot. Enqueue trừ Gỗ/Thực/Vàng trong một giao dịch, thiếu bất kỳ loại nào không trừ dở dang. TrainingTicket lưu riêng ba lượng đã trả; chuột phải icon hủy đúng ID và hoàn toàn bộ giá đó, không dùng giá mới từ asset. Hủy đầu mất tiến độ lượt đầu, lượt kế bắt đầu0; hủy lượt sau giữ tiến độ đầu. Nếu ví tràn int khi hoàn, giữ nguyên cả queue và tài nguyên, báo rõ. Móng chưa nhận lệnh; nhà bị phá mất queue/chỗ đặt và không hoàn chi phí, như TC đã có. Chỉ quân spawn thành công mới +1 dân số; cửa ra kín hoặc thiếu sức chứa mới thì chờ100%.

- Chọn nhà quân hoàn thiện bằng click footprint; giữ SelectedTrainingBuildingId dùng chung với TC, vòng chọn/cờ hiện trên BuildingView. HUD dùng cùng panel5slot/vòng tiến độ, đổi tiêu đề/tên quân/giá/thời gian theo nhà đang chọn. Chuột phải map đặt cờ cho nhà đó, áp dụng cho quân sinh sau; cờ của nhà này không đổi cờ nhà khác. Dân từ TC vẫn biết Gather khi cờ đặt lên nguồn. Quân chiến đấu có CarryCapacity0/Villager=null; đặt cờ lên cây chỉ đi tới ô gần nguồn, không khởi tạo việc khai thác hoặc gây null reference.

- Điều kiện kết thúc do ConfigureTownCenterVictory đăng ký hai TC đối đầu lúc khởi tạo (chỉ bật ở CombatScene). Simulation đánh giá sau toàn bộ giao tranh/đạn của mỗi tick: TC địch về0 → player1 Victory; TC mình về0 → Defeat; cả hai sập cùng tick → Draw. Phá nhà/xưởng/nhà quân không tự kết thúc khi TC còn. Chốt WinnerId/MatchEndTick đúng một lần, Step dừng tăng tick/mọi hoạt động, Submit từ chối lệnh sau kết thúc và không sinh quân sau khi đã chốt. Trạng thái/cấu hình kết thúc tham gia checksum; camera/UI là cục bộ.

- MatchHudView đọc kết quả và hiện overlay VICTORY/CHIẾN THẮNG, DEFEAT/THẤT BẠI hoặc DRAW/HÒA, kèm nút CHƠI LẠI. Overlay chặn click/zoom xuống map; nút nạp lại scene hiện tại với World/tài nguyên/quân/queue/kết quả mới, không giữ ván cũ. RtsGameView ngừng chạy vòng tick khi kết thúc. Đồ họa vẫn là mẫu; thêm sprite kị binh và ba hình nhà quân để phân biệt loại.

- Thiết lập scene hiện hành: GameScene vẫn bắt đầu 1TC +3Dân với ví200 Gỗ/200 Thực/100 Vàng và 3/10, có đủ7 lựa chọn xây. CombatScene bắt đầu 3Dân + giáo/cung/kị của mình (6/10), giáo/cung/kị địch và TC địch ở (12,-5). Giữ ID4/5 là giáo/cung mình, ID6/7 là giáo/cung địch; thêm kị mình ID8 ở (1.5,-0.5), kị địch ID9 ở (12.5,-0.5). Cả hai scene dùng catalog bảy công trình và HUD mới. Nút chuyển map luôn tạo ván mới; AI kinh tế/xây quân/tấn công theo đợt vẫn thuộc Mốc4.

- Cách thử: chọn Dân → nút Trại lính/Trường bắn/Chuồng ngựa → đặt móng trên đất trống → đợi100%. Click nhà hoàn thiện → đặt cờ bằng phải lên map → bấm huấn luyện, thấy trừ đúng hai loại tài nguyên → sau10 giây quân mới xuất hiện và đi tới cờ. Phải vào icon hủy để kiểm tra hoàn đủ giá. Ví đầu đủ1 nhà150Gỗ; muốn xây cả3 phải thu thêm ít nhất250Gỗ chỉ riêng giá xây (chưa tính giá quân). Trong CombatScene, chọn quân phải lên địch/TC; phá hết HP TC sẽ hiện bảng kết thúc, bấm Chơi lại để thử ván mới.

- File cấu hình chính: Data/Units/MeleeData.asset, ArcherData.asset, CavalryData.asset và Data/Buildings/BarracksData/ArcheryRangeData/StableData kèm ba TrainingData.asset. Thông số giây = DurationTicks /20; điều chỉnh trước Play. Menu GameRTS > Set up Military and Victory (action military-setup) tạo asset/nút/marker còn thiếu, nối catalog/HUD hai scene và cập nhật cấu hình mẫu của bộ ba; scene đã lưu không cần chạy lại menu để chơi.

- Kiểm tra Bước4–6: lõi C# độc lập đạt171/171 (thêm28 trường hợp), Unity EditMode209/209 và PlayMode39/39, tổng248. Các trường hợp mới bao gồm nhân5/2/làm tròn trước giáp, ba trận khắc chế, móng không huấn luyện/nhà không nhận hàng, trả giá ba loại nguyên tử/thiếu Gỗ-Thực-Vàng, hoàn tiền tràn ví giữ nguyên lượt, spawn/rally đúng quân và rally nguồn không gây lỗi cho lính, nhà bị phá mất queue, vòng xây ba nhà/huấn luyện/tập kết/thắng-thua và đảo thứ tự nhận lệnh, hòa cùng tick, dừng Step/Submit/checksum sau kết thúc. PlayMode nạp scene thật, bấm cả ba nút xây/huấn luyện, chọn nhà/cờ, hủy icon hoàn Gỗ+Thực, kiểm tra Victory/Defeat/khóa đầu vào và Restart về9 đơn vị/6 dân của ván thử. Sửa hai vấn đề trong phép kiểm tra: enum0 cần so sánh đúng kiểu và phải chờ hết khung hình tiêu thụ click đặt móng trước khi click chọn nhà; không bỏ cơ chế chặn click đặt móng của gameplay.

- Build/đối chiếu Bước4–6 ngày15/11: Windows64-bit build thành công, 0 lỗi, giữ cả GameScene và CombatScene. Replay phiên bản11 thêm hai thế giới quân sự thắng/thua vào bảy thế giới cũ; 600 bộ chín checksum Editor/Windows khớp byte đủ94280 byte, headless player exit0. Hai thế giới dùng giá xây/quân/thời gian thật, TC giảm HP riêng cho kiểm tra, không sửa HP asset gameplay. Log ở Logs/Military.editmode.xml, Military.playmode.xml, Military.build.txt, Military.editor-replay.txt, Military.windows-replay.txt, Military.player.log; bản chơi ở Builds/Windows/GameRTS.exe cùng thư mục dữ liệu.

- Xác minh giao diện ngày 15/11/2026 (ghi chép trước nghiệm thu PC): PlayMode đã bấm đúng ba nút xây, chọn nhà hoàn thiện, huấn luyện từng loại/rally, hủy icon hoàn hai loại, hiện Victory/Defeat và Chơi lại về ván mới. Lượt đó đã mở bản PC nhưng công cụ Windows trả “failed to activate captured window” ở cả lần đầu và lần chọn lại cửa sổ, nên chưa thử chuột trực tiếp. Không ghi nhận đây là lỗi gameplay; kiểm tra headless/Unity và build đều đạt. Phần thử PC còn thiếu đã được hoàn thành trong lượt bên dưới. Đây chưa phải bản đánh AI hoàn chỉnh: bước tiếp theo là Mốc4 AI kinh tế/xây quân/tấn công theo đợt.

- Nghiệm thu bằng chuột trên bản PC, kết thúc ngày 16/11/2026: mở trực tiếp GameRTS.exe ngoài Editor; dùng bản build Mốc 3 có assembly ngày 15/11, không sửa mã nguồn/asset, không build lại và không gọi trực tiếp API Simulation để tạo kết quả. GameScene hiện đúng 1 TC + 3 Dân, ví 200 Thực/200 Gỗ/100 Vàng, dân số 3/10. Chọn Dân hiện đủ bảy nút xây; đặt Trại lính lên cây bị báo đỏ/không trừ tiền, đặt trên đất trống trừ đúng 150 Gỗ và Dân xây thành nhà hoàn thiện. Tiếp tục thu Gỗ để xây Trường bắn và Chuồng ngựa, mỗi nhà 150 Gỗ; không dùng F9 nạp ví hoặc F8/chuột giữa rút nguồn.

- Huấn luyện/tập kết trên PC: từng nhà mở đúng tên quân và giá — Giáo 50 Thực +25 Gỗ, Cung 25 Gỗ +45 Vàng, Kị 70 Thực +60 Vàng. Đã quan sát mỗi loại sinh ra và tự tới cờ riêng của nhà; dân số ván kinh tế tăng từ 3/10 đến 8/10, gồm 3 Dân +3 Giáo +1 Cung +1 Kị. Khi Chuồng ngựa còn thiếu Vàng, bấm huấn luyện báo “Không đủ Vàng để huấn luyện”, queue vẫn 0 và không trừ Thực. Sau khi Dân đào/nộp đủ Vàng mới huấn luyện được Kị. Chuột phải icon Giáo thứ hai trong queue [2/5] làm còn [1/5], Thực 0→50 và hoàn 25 Gỗ; tiến độ lượt đầu vẫn tiếp tục (quan sát 73%), không bị đặt lại 0. Gỗ HUD còn tăng bởi Dân đang thu/nộp đồng thời nên không dùng chênh lệch Gỗ giữa hai ảnh để suy toàn bộ số tiền hoàn.

- Kiểm tra kinh tế đi kèm trên PC: Dân đang mang Gỗ được giao xây Trường bắn/Chuồng ngựa vẫn giữ ba lô, xây xong tự nộp và khai thác tiếp. Cả năm cây mẫu được thu hết, Dân chuyển cây và cuối cùng báo hết nguồn Gỗ trong 8 ô. Ví Gỗ còn 150 khớp 200 ban đầu +500 khai thác −450 xây ba nhà quân −75 tạo ba Giáo −25 tạo một Cung; Kị không tốn Gỗ. Sau đó chuyển Dân sang bụi quả rồi mỏ Vàng để trả đủ giá Kị. Đây là vòng kinh tế thật của ván, không phải kết quả nạp ví thử.

- Giao tranh và Victory trên PC: chuyển sang CombatScene bắt đầu 6/10; kéo khung chọn Giáo/Cung/Kị và chuột phải Cung địch, quan sát quân vòng tường, HP thay đổi, quân chết biến mất và dân số 6→3. Địch còn một Kị 93 HP. Xây Trại lính, huấn luyện hai Giáo; Giáo tiếp viện hạ Kị còn lại. Chọn hai Giáo, phải vào TC địch; quan sát nhà từ 1000 HP xuống 370 rồi sập, hiện VICTORY/CHIẾN THẮNG. Phải vào map sau kết thúc không làm hai quân di chuyển tiếp. Bấm CHƠI LẠI đưa về ví 200/200/100, 6/10, quân hai phe và hai TC nguyên vẹn; Trại lính đã xây ở ván trước không còn.

- Defeat trên PC: đưa ba quân chiến đấu mình ra xa căn cứ, cho Dân khai thác sát vùng địch để kích hoạt dò/đuổi; Dân bị đánh chết làm dân số giảm. Huấn luyện thêm một Dân, đặt cờ gần căn cứ để kéo Giáo địch vào tầm phát hiện TC. Sau khi Dân chết, Giáo địch tự chuyển sang đánh TC: quan sát 1000→870→0 HP và hiện DEFEAT/THẤT BẠI, dù ba quân mình vẫn sống ở xa (3/10). Bấm CHƠI LẠI từ màn này cũng trả về ván mới 6/10 và ví 200/200/100. Để cửa sổ PC ở CombatScene mới để người dùng chơi tiếp; không thay đổi scene đang mở trong Editor.

- Kết luận lượt PC: các luồng chọn/đặt móng/huấn luyện/hủy/tập kết/giao tranh/chết/phá TC/thắng–thua/chơi lại đã chạy được bằng chuột. Không phát hiện lỗi chặn chơi trong phạm vi này. Bản sao Player.log không có exception C# hoặc stack trace; có dòng khởi tạo D3D12 “failed to query info queue interface (0x80004002)” nhưng game vẫn hiển thị và chơi được. Nhật ký nằm tại Logs/Military.native-pc-2026-11-16.txt, runtime log tại Logs/Military.native-pc-player.log, cả hai thuộc nhóm log bị Git bỏ qua. Trường hợp Hòa cùng tick và thời điểm tên gây sát thương chính xác vẫn dựa vào bộ kiểm thử đã có, chưa tái hiện thủ công trong lượt này. Chưa thay cho chơi cân bằng nhiều trận, thử tải nhiều quân hoặc thử mạng nhiều PC.

- Giới hạn sau Bước4–6: mới một loại quân huấn luyện ở mỗi nhà, cùng thời gian mẫu10 giây, chưa có nâng cấp/thời đại/công trình tạo quân đối phương tự hoạt động. Tên vẫn truy đuổi và bay qua vật cản, giáp chung; chưa có stance/kiting tự động/animation chuẩn/fog/âm thanh. GameScene kinh tế một phe không bật màn thắng/thua. Multiplayer thực tế/multi-PC thuộc Mốc8; kiểm chứng cùng máy không thay cho thử mạng.

- Đã triển khai Bước 1–3: máu, sát thương, giáp, chết; tự tìm mục tiêu, đuổi, cận chiến và tầm xa.

- Đã triển khai Bước4–6: giáo/cung/kị và khắc chế, ba nhà quân cùng hàng huấn luyện/tập kết, điều kiện phá TC và màn kết thúc/chơi lại.

- Nội dung mở rộng về sau: pháp sư, xe công thành và lính lâu đài; dữ liệu/nâng cấp/cân bằng cho các quân này chưa được triển khai.

Chơi được: hai bên đánh nhau và phá Tòa Thị Chính.

### Mốc 4 — AI đối thủ (hoàn thành chức năng Bước 1–6 ngày 16/11/2026)

- Phạm vi triển khai theo yêu cầu ngày 16/11/2026: Bước 1 lõi FSM + fair play, Bước 2 kinh tế tự động, Bước 3 nhà quân/huấn luyện/tập kết. Có pha tấn công cơ bản để kiểm tra vòng khép kín; tại thời điểm Bước1–3 chưa đánh dấu xong toàn Mốc4. Rút lui và ba mức khó đã được bổ sung trong Bước4–6 bên dưới.

- AI PlayerId2 dùng cùng ví, nguồn hữu hạn, ba lô, đường đi, xây móng/tiến độ, giá và dân số/hàng đợi với người chơi. AiController chỉ gửi Gather/Build/ResumeBuild/Training/Move/Attack qua Submit, trễ 2 tick, không dùng Command debug hoặc cộng tiền/sinh quân trực tiếp. View chỉ khởi tạo 3 Dân đầu mỗi bên và chuyển dữ liệu profile sang C# thuần trước tick1.

- FSM ở Bước1–3: Economy → Military từ8 Dân → Attack từ6 quân gần điểm tập kết, mục tiêu10 Dân/tối đa18 quân. Bước4 bên dưới thay ngưỡng xuất kích bằng9 và thêm vòng hồi phục. Kinh tế vẫn vận hành xuyên suốt. Nhịp quyết định mặc định30 tick=1,5 giây, cấu hình20–40 tick. Tick/sequence/phase/trí nhớ/nhóm quân/config tham gia checksum; không dùng thời gian frame hoặc Random của Unity.

- Tầm nhìn chung được đưa lên sớm để thực hiện yêu cầu fair play: quân sống mở bán kính 8 ô, nhà hoàn thiện 10 ô mặc định, áp dụng như nhau cho cả hai bên trong AiScene. AI chỉ ghi nhớ nguồn khi thực sự thấy, không đọc ví đối phương; Attack cần mục tiêu đang thấy, không đuổi theo vị trí hiện tại của quân đã mất dấu, đặt móng cần toàn footprint trong tầm nhìn. View ẩn địch/nguồn/đạn ngoài vùng thấy. Đây là nền tầm nhìn, chưa có lớp phủ đen/mờ, ghi nhớ sprite hoặc che khuất bởi vật cản; các phần trình bày sương mù đầy đủ vẫn thuộc Mốc 5. GameScene và CombatScene giữ chế độ prototype cũ khi không bật Vision.

- Kinh tế: chia số Dân đang sẵn sàng thu theo 40% Thực /40% Gỗ /20% Vàng, dùng phần dư để phân đủ số nguyên (3 Dân: mỗi loại 1; 5 Dân: 2 Thực/2 Gỗ/1 Vàng; 10 Dân: 4/4/2). Giữ công việc đang hiệu quả và hoãn chuyển loại khi đang mang hàng để hạn chế đi trả hàng liên tục. Dân xây tạm tách khỏi nhóm phân công; xây xong dùng lại vòng nộp/thu hiện có. TC rảnh/đủ tiền/chỗ thì đẻ Dân đến mục tiêu; còn ≤2 chỗ sau khi tính mọi lượt đang xếp thì ưu tiên một Nhà, không đặt trùng móng. Thợ bị giết có thể được thay bằng ResumeBuild, không mua móng lần nữa.

- Xưởng và nhà quân: chọn nguồn đã biết gần TC, xây Xưởng Gỗ/Xưởng Vàng bằng 100 Gỗ và đứng thợ xây đủ công; chỉ xưởng hoàn thiện mới nhận hàng. Tìm footprint trống trong 10 ô quanh TC, chừa đường giữa nhà và không bịt các ô sát nguồn/điểm tập kết. Ba nhà quân giá 150 Gỗ dùng dữ liệu hiện có. Đếm quân sống + đang xếp hàng theo lớp để huấn luyện cân bằng; mỗi nhà đặt cờ về điểm tập kết. Lính nhàn chưa thuộc đợt tấn công mà cách điểm này hơn 5 ô được gọi về tập kết. Giá, thời gian 200 tick và sức chứa hàng đợi 5 dùng hệ chung; thiếu tài nguyên/chỗ thì chờ, không bỏ qua luật.

- Pha tấn công cơ bản ở Bước1–3: nhóm đủ quân rời căn cứ bằng Move để trinh sát, chỉ Attack vào địch/nhà đã thấy. Điểm tìm kiếm đầu tiên suy từ căn cứ mình và hình học bản đồ đối xứng; không lấy ID/vị trí TC địch khi chưa thấy. Chưa thấy mục tiêu thì tiếp tục tìm vùng chưa khám phá. Rút lui/tự vệ được bổ sung tại Bước4; lựa đội hình theo khắc chế, bảo vệ Dân theo mức đe dọa ; ba độ khó được bổ sung ở Bước5.

- AiScene là map riêng 64×64, hai bên cùng 1 TC +3 Dân, ví 200 Gỗ/200 Thực/100 Vàng và 3/10; tài nguyên đối xứng, mỗi bên 12 cây×100, 8 bụi quả×150, 4 mỏ×800. Không cấp sẵn quân chiến đấu/nhà quân cho AI; công cụ nạp ví/rút nguồn bị tắt trong scene này. Nút ĐẤU VỚI MÁY vào AiScene; nút VỀ BẢN ĐỒ KINH TẾ quay lại GameScene, mỗi lần là ván mới. AiStandard.asset lưu các mục tiêu/nhịp nghĩ mẫu, chưa phải cấu hình cân bằng cuối.

- Kiểm thử tự động đã đạt: **189/189 kiểm tra lõi C# độc lập, 229/229 EditMode và 42/42 PlayMode (271 Unity)**. Gồm phân công thật 10 Dân 4/4/2, lịch nghĩ 20/30/40 tick, không cấp tiền/quân, giới hạn dân số, giấu thông tin ngoài tầm nhìn, gọi quân lẻ về tập kết, thay thợ chết và bảo toàn tài nguyên kể cả chi phí/hàng đợi/ba lô. Hai lần mô phỏng cùng đầu vào có checksum trùng; AI xây đủ hai xưởng/ba nhà quân, tạo đủ Giáo/Cung/Kị và phá TC bên đứng yên ở tick4194 (khoảng 210 giây) trong kịch bản lõi. PlayMode dùng scene thật xác nhận vòng kinh tế/quân sự/tấn công, DEFEAT, Chơi lại và các nút chuyển map; kiểm thử cũ của Mốc 1–3 cũng đạt.

- Build Windows 64-bit thành công, **0 lỗi**. Replay phiên bản12 gồm 600 hàng×10 checksum; riêng AI chạy10 tick/hàng để kiểm tra tối đa6.000 tick. Bản headless thoát mã0, Editor/Windows khớp byte đủ104.505 byte, SHA256 `6c487846d6c56a8911ff87a84e0ce854759dca63eb2ec67787afda98b88fb5c5`. Bằng chứng nằm trong `Logs/Ai.*`; đối chiếu cùng máy chưa thay cho nghiệm thu lockstep trên nhiều máy ở Mốc8.

- Thử trực tiếp `Builds/Windows/GameRTS.exe` ngày 16/11/2026: từ GameScene bấm ĐẤU VỚI MÁY, thấy TC mình 1.000 HP/3 Dân, ví200 Thực/200 Gỗ/100 Vàng và dân số3/10; địch không hiển thị ngoài tầm nhìn. Giữ bên người chơi đứng yên, không tạo tài nguyên/quân hoặc sửa HP. Sau khi AI tự phát triển, thấy sáu quân AI gồm Giáo/Cung/Kị vào căn cứ, Dân chết làm HUD còn0/10, TC xuống740/1.000 HP rồi sập và hiện DEFEAT/THẤT BẠI. Ví mình vẫn200/200/100. Bấm CHƠI LẠI tạo ván mới đúng3/10, TC đầy máu và ví ban đầu, không giữ quân địch đang đánh. Bấm VỀ BẢN ĐỒ KINH TẾ đưa về GameScene, hiện lại hai nút thử chiến đấu/đấu máy. Bàn giao bản PC ở GameScene; Editor cũng đã được đưa về GameScene. Nhật ký/runtime log thuộc nhóm `Logs/Ai.native-pc-*`. Vòng kinh tế/xây/huấn luyện chi tiết bên AI và tính công bằng được kiểm tra bằng lõi/PlayMode; lượt chuột này xác nhận giao diện, quân tấn công, kết thúc và khởi động lại thực tế.

#### Bước 4 — Xuất kích theo đợt, tự vệ và tái tổ chức (hoàn thành ngày16/11/2026)

- Mẫu hiện hành: WaveSize=9 lính (có thể đổi8–12 trong AiStandard.asset), chỉ quân sống, đã đứng yên trong5 ô quanh điểm tập kết và không đang đánh mới được tính. Khi đủ ngưỡng, lấy toàn bộ nhóm sẵn sàng làm một đợt, lưu ID/quân số ban đầu. Lính sinh sau tiếp tục gom ở căn cứ, không trộn vào đợt đang đi để che mất số tổn thất.

- Khi xuất kích, chỉ xét mục tiêu hiện đang thấy: lính địch → Dân địch → TC → công trình khác, rồi khoảng cách và ID để phân xử ổn định. Đã bao gồm nhà không nhận tài nguyên như Nhà/nhà quân. Không có mục tiêu thì trinh sát bằng Move như trước; mục tiêu mất dấu không được theo dõi bằng vị trí bí mật. Chỉ gửi Attack cho quân cần đổi đích để tránh liên tục làm lại đường của quân đang đánh đúng mục tiêu.

- Đợt bị đánh tan khi số quân sống còn≤1/3 quân số ban đầu (9 còn3, hoặc chết hết). AI gửi Move gọi số sống về điểm tập kết, xóa đợt hiện hành và chuyển Phase=Economy/IsRegrouping. Kinh tế, đẻ Dân, xây Nhà và sản xuất quân bằng giá thật vẫn hoạt động để hồi phục. Chờ ít nhất RegroupTicks=200 tick/10 giây và quân sống về ổn định, rồi qua Military, đủ9 mới xuất kích tiếp. Nếu quân về bị kẹt, sau thêm400 tick/20 giây cho phép gom nhóm mới thay vì khóa AI vĩnh viễn; quân kẹt vẫn tiếp tục được gọi về khi có thể. Đây là gom/huấn luyện bổ sung, chưa có cơ chế hồi máu.

- Tự vệ: có quân hoặc Dân địch hiện trong tầm nhìn và trong BaseRadius+2=12 ô quanh TC thì quân chiến đấu AI được giao đánh kẻ xâm nhập, không cần đủ9. Nếu đang có đợt xuất kích thì hủy đợt/gọi quân về qua vòng hồi phục. Ưu tiên lính địch trước Dân; Dân AI vẫn làm kinh tế, không tự biến thành quân. Khi hết địch gần căn cứ, quân trở về điểm tập kết trước khi xuất kích tiếp. Chưa có đánh giá sức mạnh tổng hợp, kiting, chia quân giữ nhà hoặc né đòn.

- AiController.Warfare.cs là phần điều quân C# thuần của AiController, không phải MonoBehaviour mới. AiState giữ số đợt, quân số gốc, cờ/thời hạn và ID quân về; SimulationWorld.Ai đưa toàn bộ vào checksum. AiProfileSO/AiStandard.asset thêm RegroupTicks; scene vẫn dùng asset chung nên không cần tạo lại AiScene. Replay13 có hai thế giới AI, trong đó một fixture thêm lính phòng thủ mạnh chỉ để thử đánh tan/hồi phục; không thay chỉ số hoặc cấp lính đó vào map chơi thật.

- Kiểm chứng Bước4: **198/198 kiểm tra lõi độc lập, 238/238 EditMode +42/42 PlayMode (280 Unity) đạt**. Chín trường hợp mới gồm8 chờ/9 xuất kích, quân bổ sung không trộn đợt, lính→Dân→TC→nhà khác, quân bí mật không ảnh hưởng lựa chọn, tự vệ khi chỉ có3 lính, gọi đủ đợt9 quân đang đi về cứu căn cứ, mất6/9 thì gọi3 sống về, tiếp tục kinh tế/sản xuất để mở đợt thứ hai, config hồi phục nằm trong checksum. Kịch bản bị đẩy lùi chạy hai thế giới tối đa12.000 tick và so checksum mỗi nhịp quyết định; bản AI đầu đã tạo lại đợt ít nhất9 quân, còn10 Dân và không vượt dân số. PlayMode dùng scene thật xác nhận đủ9 lính trước đợt đánh, AI phá TC, overlay DEFEAT và Restart sạch. Khi xây fixture rút lui, lệnh Move của test ban đầu bị lệnh xuất kích tick+2 ghi đè; đã sửa thời điểm gửi fixture, không giảm tiêu chí gameplay.

- Windows64-bit build thành công, **0 lỗi**; replay13 đủ600 hàng×11 checksum Editor/Windows khớp byte (**114.705 byte**), SHA256 `493682016868d2305803188fc214fedde0848f6af8ec7183934d5ccf11201ee9`, headless exit0. Hai thế giới AI mỗi thế giới bước20 tick/hàng; fixture lính phòng thủ mạnh chỉ thuộc test/replay, không có trong AiScene. Bằng chứng tại `Logs/AiWaves.*`; chưa thay cho nghiệm thu mạng/nhiều cấu hình PC.

- Thử chuột trực tiếp bản PC mới: mở GameRTS.exe, bấm ĐẤU VỚI MÁY, khởi đầu đúng 1 TC đầy 1.000 HP +3 Dân, ví 200 Thực/200 Gỗ/100 Vàng và 3/10; địch ngoài tầm nhìn được ẩn. Không điều quân bên mình, không dùng công cụ debug. Sau khi AI phát triển, quan sát 9 lính gồm Giáo/Cung/Kị vào căn cứ và tấn công: Dân bị giết, dân số xuống 0/10, TC mất máu rồi sập, hiện DEFEAT/THẤT BẠI; ví người chơi vẫn 200/200/100. Bấm CHƠI LẠI thấy ván mới có 3 Dân, TC đầy máu, nguồn đầy và ví ban đầu, không giữ quân đang xâm nhập. Lần quan sát sau khi tiếp tục công việc không còn cửa sổ PC; bản sao runtime log có kết thúc Unity bình thường và không có exception C#. Không ghi nhận đã thử rút lui/tự vệ/đợt hai bằng chuột: các hành vi này có bằng chứng lõi/Unity/replay riêng ở trên. Editor đã được đưa về GameScene sau kiểm thử. Nhật ký: `Logs/AiWaves.native-pc-2026-11-16.txt`; runtime: `Logs/AiWaves.native-pc-player.log`.

- Cách thử: vào ĐẤU VỚI MÁY, xây kinh tế/nhà quân và gom quân phòng thủ. AI mất nhiều thời gian chuẩn bị hơn bản6 lính cũ; đánh bại đợt đầu rồi quan sát quân sống quay về và đợt tiếp theo. Có thể mở AiStandard.asset trước Play để chỉnh Wave Size/Regroup Ticks; thay đổi giữa ván không cập nhật config Simulation đã chụp lúc bắt đầu.

- Ở thời điểm hoàn thành Bước4, ba độ khó còn thiếu (đã bổ sung ở Bước5). Các phần tiếp tục mở rộng: đánh giá tương quan sức mạnh/đe dọa, phân đội phòng thủ và cân bằng nhiều ván. Sương mù đen/mờ đầy đủ và multiplayer vẫn ở các mốc sau.

Chơi được: một ván hoàn chỉnh trên map 1, đánh với máy.

#### Bước 5–6 — Độ khó và ván PvE (hoàn thành chức năng ngày 16/11/2026)

- Vào ĐẤU VỚI MÁY mở panel chọn Dễ/Vừa/Khó, mặc định Vừa; BẮT ĐẦU mới nạp AiScene. QUAY LẠI đóng panel. Chơi lại giữ mức đã chọn, về GameScene xóa lựa chọn cũ. Mở trực tiếp AiScene trong Editor vẫn dùng AiStandard.asset thử 9 quân để giữ kịch bản kiểm tra độc lập.
- Dễ: 40 tick/2 giây, 8 Dân, từ8 Dân xây quân, đợt6 quân, giới hạn12 lính, hồi phục20 giây, nghỉ tối thiểu60 giây sau đợt bị đẩy lùi. Không có nhóm quấy rối riêng.
- Vừa: 30 tick/1,5 giây, 10 Dân, từ8 Dân xây quân, đợt11 quân, giới hạn22 lính, hồi phục10 giây, khoảng đợt tối thiểu20 giây; sản xuất cân bằng Giáo/Cung/Kị.
- Khó: 20 tick/1 giây, 12 Dân, từ5 Dân ưu tiên xây nhà quân trước xưởng và đẻ thêm Dân khi đủ giá; điều chỉnh một phần Dân theo thiếu Gỗ/Thực/Vàng. Đợt15+ quân, giới hạn30 lính, hồi phục5 giây, không thêm thời gian nghỉ giữa đợt. Chia hai nhóm chẵn/lẻ đi hai điểm vòng ở hai phía căn cứ đối xứng trong15 giây trước khi giao chiến chung. Đây là chiến thuật mẫu, chưa có phân tích địa hình/chọn đường vòng tối ưu.
- Mọi mức dùng ví ban đầu, sức chứa, nguồn hữu hạn, giá, thời gian huấn luyện, tầm nhìn và bộ Command giống người chơi. Không tăng tiền, HP, tốc độ khai thác hoặc cấp sẵn quân cho Hard. Tối ưu nhanh hơn dựa trên quyết định/phân công và số Dân thật.
- Replay14 thêm ba thế giới Easy/Medium/Hard vào11 thế giới cũ, tổng600 hàng×14 checksum. Các thế giới AI bước20 tick/hàng, tối đa12.000 tick.
- Kiểm thử bản cuối: 205/205 lõi C# độc lập; 245/245 EditMode +43/43 PlayMode (288 Unity) đạt. Cả ba mức tự phát triển/xuất kích/phá TC bên đứng yên; Hard gửi hai Move cho hai nhóm không trùng ID. PlayMode bấm menu chọn từng mức/Bắt đầu, kiểm tra đúng cấu hình và3 Dân ban đầu.
- Kịch bản PvE tự động: player1 và AI2 cùng bắt đầu TC+3 Dân/ví chuẩn. Controller kiểm thử bên player1 dùng Command thường với chiến thuật8 Dân/ra quân6 sớm, xây nhà quân/thu/nộp/huấn luyện bằng giá thật, giành WinnerId=1 trước AI Dễ. Không cộng tiền, cấp quân, giảm HP TC hoặc tăng sức mạnh. Đây là kiểm thử tự động bên người chơi, không ghi là đã chơi trọn ván bằng chuột. GUI Victory/Defeat/Chơi lại đã có kiểm thử Mốc3–4. Cân bằng thắng thua qua nhiều ván vẫn còn ở Mốc7.
- Build Windows0 lỗi; replay14 Editor/Windows khớp145.305 byte (SHA256 ở đầu tài liệu), headless exit0. Thử chuột PC xác nhận giao diện ba mức/mô tả, chọn Dễ/Bắt đầu đúng3 Dân và quay về/mở lại mặc định Vừa; runtime không có exception C#. PC để tại menu chọn mức, Editor về GameScene. Nhật ký `Logs/Difficulty.native-menu.txt`; bản sao log `Logs/Difficulty.native-player.log`. Chưa chơi trọn trận thắng bằng chuột trong lượt này; tiêu chí thắng được xác minh bằng kịch bản tự động kinh tế thật, không phải ép HP hay kết quả.
- Bản core đầu dùng chiến thuật player1 đợi15 quân đã thua; phép nghiệm thu được sửa sang chiến thuật đánh sớm6 quân hợp lệ, không giảm độ khó hay cấp lợi thế chỉ số. Không dùng một trận tự động làm bằng chứng mọi chiến thuật đều thắng.

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

- [x] Chốt nền tảng PC, góc nhìn 2D top-down, kiến trúc Simulation/View + Command và hướng lockstep.

- [x] Camera, chọn quân, di chuyển nhóm, pathfinding 8 hướng và dữ liệu đơn vị.

- [x] Ví/HUD, nguồn hữu hạn, thu–mang–nộp, Nhà/ba xưởng, xây dựng và huấn luyện Dân/tập kết — đủ 6 bước Mốc 2.

- [x] Chiến đấu và 3 quân đầu, khắc chế, 3 nhà quân/huấn luyện, phá TC và thắng–thua/chơi lại — đủ 6 bước lõi Mốc 3, đã kiểm thử tự động.

- [x] Chơi thử bằng chuột trên bản PC sau Mốc 3 Bước 4–6 — đạt ngày 16/11/2026, gồm Victory, Defeat và Chơi lại từ cả hai màn.

- [x] AI cơ bản Mốc 4 Bước 1–3 — FSM/fair play, kinh tế tự động, ba nhà quân/sản xuất/tập kết và đợt tấn công; kiểm thử Unity/build/replay và thử PC trực tiếp đạt ngày 16/11/2026.

- [x] Mốc 4 Bước 4 — đợt 9 lính, ưu tiên mục tiêu, tự vệ và rút/gom lại: lõi/Unity/build/replay đạt ngày 16/11/2026; PC trực tiếp xác nhận đợt 9 quân, DEFEAT và Chơi lại.

- [x] Mốc4 Bước5–6: ba mức khó, menu chọn trận, kiểm thử PvE kinh tế thật đến Victory cho player1.

- [ ] Chơi cân bằng nhiều ván bằng chuột ở cả ba mức; đánh giá tương quan sức mạnh/đe dọa nâng cao.

- [ ] Thêm 3 quân còn lại (pháp sư, xe công thành, lính lâu đài), nâng cấp và thời đại.

- [ ] Sương mù, công trình phòng thủ, thú và cá.

- [ ] UI hoàn chỉnh, âm thanh, hiệu ứng — hiện đã có HUD gameplay và bảng kết thúc làm nền.

- [ ] Map 2–5, cân bằng, tối ưu.

- [ ] Phát hành bản đánh máy.

- [ ] Multiplayer lockstep: truyền lệnh, lobby, kết nối và kiểm thử nhiều máy.
