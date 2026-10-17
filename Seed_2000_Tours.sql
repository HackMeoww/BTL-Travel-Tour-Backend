-- ========================================================================
-- SCRIPT SEED 2000 DỮ LIỆU TOUR DU LỊCH MẪU
-- Database: TravelTourDB
-- Ngày tạo: 2026-10-14
-- ========================================================================

USE TravelTourDB;


SET NOCOUNT ON;

PRINT N'>> Bắt đầu sinh 2000 tour du lịch mẫu...';

BEGIN TRANSACTION;

BEGIN TRY
    -- 1. Bảng số thứ tự từ 1 đến 2000
    ;WITH Numbers AS (
        SELECT TOP (2000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N
        FROM sys.all_objects a
        CROSS JOIN sys.all_objects b
    ),
    -- 2. Danh mục 25 điểm đến trong và ngoài nước nổi tiếng kèm tọa độ thực tế
    Destinations AS (
        SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS DestId, * FROM (VALUES
            (N'Đà Nẵng', 16.0544, 108.2022, N'thành phố đáng sống, bãi biển Mỹ Khê tuyệt đẹp, Bà Nà Hills và Cầu Rồng phun lửa.'),
            (N'Phú Quốc', 10.2899, 103.9840, N'đảo ngọc thiên đường, bãi Sao cát trắng mịn, Grand World không ngủ và cáp treo Hòn Thơm.'),
            (N'Nha Trang', 12.2388, 109.1967, N'vịnh biển ngọc lung linh, VinWonders đẳng cấp quốc tế, tháp Bà Ponagar và lặn ngắm san hô.'),
            (N'Đà Lạt', 11.9404, 108.4583, N'xứ sở ngàn hoa mộng mơ, không khí se lạnh, săn mây đồi chè Cầu Đất và cà phê view thung lũng.'),
            (N'Sapa', 22.3364, 103.8438, N'chinh phục đỉnh Fansipan 3.143m nóc nhà Đông Dương, thung lũng Mường Hoa và bản Cát Cát thơ mộng.'),
            (N'Hạ Long', 20.9505, 107.0734, N'kỳ quan thiên nhiên thế giới, du thuyền ngủ đêm 5 sao sang trọng, hang Sửng Sốt và đảo Ti Tốp.'),
            (N'Hà Nội', 21.0285, 105.8542, N'trái tim thủ đô nghìn năm văn hiến, 36 phố phường nhộn nhịp, hồ Hoàn Kiếm và ẩm thực tinh hoa.'),
            (N'Hội An', 15.8801, 108.3380, N'di sản văn hóa rực rỡ sắc đèn lồng, chùa Cầu cổ kính, chèo thuyền thả hoa đăng trên sông Hoài.'),
            (N'Huế', 16.4637, 107.5909, N'cố đô thâm nghiêm cổ kính, Đại Nội uy nghi, lăng tẩm triều Nguyễn và nghe ca Huế trên sông Hương.'),
            (N'Ninh Bình', 20.2506, 105.9745, N'quần thể di sản Tràng An non nước hữu tình, Tam Cốc Bích Động, chùa Bái Đính và đỉnh Hang Múa.'),
            (N'Quy Nhơn', 13.7820, 109.2197, N'bãi biển Kỳ Co trong vắt, Eo Gió ngắm hoàng hôn kỳ vĩ và thưởng thức hải sản tươi ngon đầm Thị Nại.'),
            (N'Hà Giang', 22.8233, 104.9839, N'chinh phục tứ đại đỉnh đèo Mã Pí Lèng, hẻm vực Tu Sản bên dòng sông Nho Quế và mùa hoa tam giác mạch.'),
            (N'Vũng Tàu', 10.3460, 107.0843, N'bãi Trước bãi Sau thoáng mát, ngọn hải đăng lâu đời, tượng Chúa Kito và ẩm thực bánh khọt nức tiếng.'),
            (N'Côn Đảo', 8.6835, 106.6062, N'vùng đất thiêng liêng huyền bí, bãi Đầm Trầu hoang sơ tuyệt mỹ, xem rùa đẻ trứng và lặn biển.'),
            (N'Mũi Né', 10.9333, 108.2833, N'đồi cát bay kỳ ảo, cung đường biển Bàu Trắng, suối Tiên huyền thoại và hải sản tươi sống làng chài.'),
            (N'Phú Yên', 13.0882, 109.3090, N'xứ sở hoa vàng trên cỏ xanh, gành Đá Đĩa kỳ quan độc nhất vô nhị, tháp Nghinh Phong và hải sản đầm Ô Loan.'),
            (N'Cần Thơ', 10.0452, 105.7469, N'thủ phủ miền Tây sông nước miệt vườn trù phú, chợ nổi Cái Răng tấp nập buổi sớm và bến Ninh Kiều.'),
            (N'Quảng Bình', 17.4674, 106.6033, N'vương quốc hang động kỳ vĩ Phong Nha - Kẻ Bàng, động Thiên Đường tráng lệ và suối Nước Moọc.'),
            (N'Buôn Ma Thuột', 12.6667, 108.0500, N'thủ phủ cà phê đại ngàn Tây Nguyên, thác Dray Nur hùng vĩ và trải nghiệm cưỡi voi hồ Lắk.'),
            (N'Cát Bà', 20.7282, 107.0519, N'đảo ngọc vịnh Lan Hạ, bãi tắm Cát Cò nước trong xanh và trekking vườn quốc gia sinh thái.'),
            (N'Bangkok', 13.7563, 100.5018, N'xứ sở Chùa Vàng rực rỡ, cung điện Grand Palace lộng lẫy, dạo thuyền sông Chao Phraya và chợ đêm.'),
            (N'Singapore', 1.3521, 103.8198, N'quốc đảo sư tử văn minh hiện đại, Marina Bay Sands, công viên Gardens by the Bay và đảo Sentosa.'),
            (N'Seoul', 37.5665, 126.9780, N'xứ sở kim chi lãng mạn, cung điện Gyeongbokgung cổ kính, tháp N Seoul và thiên đường mua sắm Myeongdong.'),
            (N'Tokyo', 35.6762, 139.6503, N'thủ đô mặt trời mọc hiện đại bậc nhất thế giới, ngắm núi Phú Sĩ hùng vĩ, tháp Tokyo và đền Asakusa.'),
            (N'Bali', -8.4095, 115.1889, N'hòn đảo thiên đường nhiệt đới Indonesia, cổng trời Lempuyang, ruộng bậc thang Tegalalang và biển Kuta.')
        ) AS D(DestName, BaseLat, BaseLng, DestDesc)
    ),
    -- 3. Danh mục 8 phong cách tour du lịch
    Prefixes AS (
        SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS PrefId, * FROM (VALUES
            (N'Tour Du Lịch', N'Hành trình trọn gói khám phá'),
            (N'Khám Phá', N'Trải nghiệm tuyệt vời và đáng nhớ tại'),
            (N'Nghỉ Dưỡng', N'Kỳ nghỉ dưỡng thư thái, dịch vụ tiêu chuẩn đẳng cấp tại'),
            (N'Hành Trình Di Sản', N'Chiêm ngưỡng cảnh sắc tuyệt mỹ và văn hóa đặc trưng'),
            (N'Tour VIP Cao Cấp', N'Chuyến du lịch sang trọng tiêu chuẩn khách sạn 4-5 sao tại'),
            (N'Du Lịch Sinh Thái', N'Hòa mình vào thiên nhiên tươi đẹp, không khí trong lành tại'),
            (N'Kỳ Nghỉ Vàng', N'Trọn gói ưu đãi hấp dẫn dành cho kỳ nghỉ dưỡng gia đình tại'),
            (N'Check-in Trọn Gói', N'Săn trọn những góc ảnh đẹp mê hồn và thưởng thức đặc sản nức tiếng tại')
        ) AS P(PrefixTitle, ActionDesc)
    ),
    -- 4. Tạo 2000 bản ghi chi tiết
    GeneratedTours AS (
        SELECT 
            -- Tên Tour: Phong cách + Điểm đến + Thời lượng + Mã số tour
            P.PrefixTitle + N' ' + D.DestName + N' ' + 
            CAST(((N * 3 + 1) % 6) + 2 AS NVARCHAR(10)) + N'N' + 
            CAST(((N * 3 + 1) % 6) + 1 AS NVARCHAR(10)) + N'Đ' +
            N' (Mã #' + RIGHT('000' + CAST(N AS NVARCHAR(10)), 4) + N')' AS TourName,

            D.DestName AS Destination,

            -- Mô tả chi tiết
            P.ActionDesc + N' ' + D.DestName + N' - ' + D.DestDesc + 
            N' Tour trọn gói đã bao gồm xe đưa đón, vé máy bay/du thuyền, phòng khách sạn tiện nghi và bảo hiểm du lịch suốt tuyến.' AS Description,

            -- Mức giá từ 1.200.000 VNĐ đến 28.000.000 VNĐ
            CAST((1200000 + ((N * 137) % 240) * 100000) AS DECIMAL(18, 2)) AS Price,

            -- Thời lượng từ 2 đến 7 ngày
            (((N * 3 + 1) % 6) + 2) AS Duration,

            -- Tọa độ chuẩn của điểm đến kèm độ lệch tự nhiên nhỏ
            CAST(D.BaseLat + (((N * 7) % 100 - 50) * 0.0015) AS DECIMAL(9, 6)) AS Latitude,
            CAST(D.BaseLng + (((N * 11) % 100 - 50) * 0.0015) AS DECIMAL(9, 6)) AS Longitude
        FROM Numbers N
        JOIN Destinations D ON D.DestId = ((N - 1) % 25) + 1
        JOIN Prefixes P ON P.PrefId = ((N - 1) % 8) + 1
    )
    -- 5. Thực hiện Insert vào bảng Tours
    INSERT INTO Tours (TourName, Destination, Description, Price, Duration, Latitude, Longitude)
    SELECT TourName, Destination, Description, Price, Duration, Latitude, Longitude
    FROM GeneratedTours;

    COMMIT TRANSACTION;

    DECLARE @TotalCount INT;
    SELECT @TotalCount = COUNT(*) FROM Tours;
    PRINT N'>> Thành công! Đã chèn 2000 tour mẫu. Tổng số tour hiện có trong bảng Tours: ' + CAST(@TotalCount AS NVARCHAR(20));
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'>> Lỗi khi sinh dữ liệu: ' + ERROR_MESSAGE();
END CATCH;

