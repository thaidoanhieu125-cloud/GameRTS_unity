# Quy ước làm việc cho dự án game RTS

Các quy định dưới đây áp dụng cho mọi lần làm việc trong dự án `GameRTS`, kể cả tiếp tục ở một cuộc trò chuyện mới. Người dùng yêu cầu duy trì các quy định này ngày 13/11/2026.

## Trước khi bắt đầu

1. Đọc lại file `AGENTS.md` này.
2. Đọc toàn bộ bản hướng dẫn và kế hoạch gốc `Game RTS Unity Doc.docx`, bao gồm tất cả các bảng. Không dùng lịch sử chat hoặc bản tóm tắt thay cho việc đọc file hiện tại.
3. Đọc `CAU_TRUC_DU_AN.md` để biết các thư mục, file và chức năng hiện có, sau đó kiểm tra mã nguồn và asset liên quan đến yêu cầu.
4. Đối chiếu yêu cầu hiện tại với kế hoạch, xác định mốc và phạm vi sẽ làm. Nói ngắn gọn cho người dùng biết phần sắp triển khai trước khi sửa dự án.

## Kế hoạch gốc

- Tài liệu thiết kế và lộ trình chính thức là `Game RTS Unity Doc.docx` ở thư mục gốc này, đường dẫn đầy đủ `D:\HocTap\PhatTrienGame\GameRTS\Game RTS Unity Doc.docx`.
- Trước mỗi lần thực hiện công việc cho game RTS, đọc lại nội dung hiện tại của tài liệu, bao gồm các bảng. Không chỉ dựa vào lịch sử chat hoặc bản tóm tắt cũ.
- Dùng tài liệu này để xác định phạm vi, luật chơi, kiến trúc và mốc công việc đang thực hiện.
- Khi người dùng yêu cầu thay đổi kế hoạch, thiết kế hoặc lộ trình, cập nhật trực tiếp đúng phần trong file DOCX gốc để phản ánh quyết định mới nhất. Với thay đổi triển khai làm thay đổi thiết kế hoặc tiến độ, cập nhật phần liên quan trong cùng tài liệu.
- Không tạo một bản kế hoạch khác thay thế hoặc mâu thuẫn với file gốc. File AGENTS.md này chỉ quy định cách làm việc.
- Các mục còn ghi là lựa chọn, ví dụ hoặc gợi ý trong tài liệu chưa phải quyết định đã chốt. Nếu yêu cầu mới của người dùng thay đổi kế hoạch, thực hiện yêu cầu mới và đồng bộ vào tài liệu.
- Khi chỉnh DOCX, giữ nguyên các phần không liên quan, bảo toàn bảng và định dạng, đồng thời kiểm tra nội dung và bố cục sau khi sửa theo kỹ năng tài liệu.
- Nếu không đọc được tài liệu, báo rõ trở ngại trước khi thực hiện công việc phụ thuộc vào kế hoạch.

## Giải thích trong và sau khi làm

- Khi triển khai, giải thích theo từng phần để người dùng hiểu đang tạo gì, vì sao cần nó và nó kết nối với phần nào của game.
- Sau mỗi bước hoặc mốc hoàn thành, bắt buộc giải thích chi tiết bằng tiếng Việt, phù hợp với người đang học Unity. Không chỉ thông báo đã xong hoặc liệt kê tên file.
- Phần giải thích khi hoàn thành phải nêu: kết quả nhìn thấy được; cách hoạt động và luồng xử lý; các file tạo mới hoặc chỉnh sửa cùng chức năng; cách mở, chạy thử và chỉnh thông số trong Unity; kết quả kiểm tra; những phần còn thiếu hoặc giới hạn liên quan và bước tiếp theo.
- Giải thích thuật ngữ kỹ thuật khi cần dùng, kèm ví dụ cụ thể từ tính năng vừa làm. Nêu rõ phần đang chạy thử, số liệu mẫu và phần đã triển khai thực tế.
- Chỉ báo đã chạy kiểm thử, đã build hoặc đã quan sát trong game khi đã thực hiện và có kết quả. Nếu có trở ngại, nói rõ phần đã xác minh được và phần chưa xác minh.

## Duy trì tài liệu cấu trúc dự án

- File mô tả cấu trúc chính thức là `CAU_TRUC_DU_AN.md`, nằm cùng thư mục với `AGENTS.md` và kế hoạch gốc.
- Mỗi khi thêm một file được giữ lại trong dự án, phải ghi vào `CAU_TRUC_DU_AN.md` ngay trong cùng lượt làm việc, trước khi báo hoàn thành. Áp dụng cho mã nguồn, Assembly Definition, scene, Prefab, dữ liệu ScriptableObject, ảnh, âm thanh, cấu hình, kiểm thử, công cụ và tài liệu.
- Mỗi file do dự án quản lý cần có đường dẫn tính từ gốc `GameRTS`, chức năng cụ thể và nơi sử dụng hoặc quan hệ với phần liên quan. Với nhóm mới, bổ sung cả vai trò thư mục và luồng hoạt động khi cần.
- Khi đổi tên, di chuyển, xóa hoặc thay đổi đáng kể chức năng của file, cập nhật mục tương ứng, cây thư mục và phần giải thích để tài liệu phản ánh cấu trúc đang có.
- Phải ghi rõ phần nào đã triển khai và phần nào mới là định hướng. Không mô tả thư mục trống hoặc Assembly Definition như thể đã có đầy đủ hệ thống gameplay.
- Các file `.meta` Unity tự sinh được mô tả theo nhóm, đi cùng asset hoặc thư mục tương ứng; ghi riêng nếu chủ động chỉnh và thay đổi đó ảnh hưởng cách dùng asset. Cache, log, file IDE và sản phẩm build tự sinh được giải thích theo nhóm thư mục hoặc loại file, không chép danh sách từng file cache vào tài liệu.
- Trước khi kết thúc, so danh sách file thêm, sửa, đổi tên và xóa với `CAU_TRUC_DU_AN.md`, kiểm tra đường dẫn tồn tại và phần mô tả còn đúng.
- `CAU_TRUC_DU_AN.md` dùng để giải thích cấu trúc hiện tại. Thiết kế, luật chơi, lộ trình và tiến độ chính thức vẫn cập nhật trong `Game RTS Unity Doc.docx`.

## Điều kiện hoàn thành một lượt làm việc

1. Hoàn thành phần công việc được yêu cầu và kiểm tra phù hợp với thay đổi.
2. Cập nhật kế hoạch gốc nếu thiết kế, lộ trình hoặc tiến độ thay đổi.
3. Cập nhật `CAU_TRUC_DU_AN.md` nếu có file mới hoặc thay đổi cấu trúc, chức năng file.
4. Giải thích chi tiết kết quả và cách người dùng tự kiểm tra theo quy định phía trên.

