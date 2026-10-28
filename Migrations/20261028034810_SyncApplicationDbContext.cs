using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Secure_Academic_Management.Migrations
{
    /// <inheritdoc />
    public partial class SyncApplicationDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayKiemTraHoSoUtc",
                table: "SinhViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTiepNhanUtc",
                table: "SinhViens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NguoiTiepNhanId",
                table: "SinhViens",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrangThaiHoSo",
                table: "SinhViens",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayHieuLuc",
                table: "LichSuTrangThais",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "ChuongTrinhDaoTaos",
                columns: table => new
                {
                    MaChuongTrinh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenChuongTrinh = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TrinhDo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DonViQuanLy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DangTuyenSinh = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuongTrinhDaoTaos", x => x.MaChuongTrinh);
                });

            migrationBuilder.CreateTable(
                name: "HoSoTotNghieps",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NgayTotNghiep = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaSinhVien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaThamChieu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HeThongNguon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayLienKetUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoSoTotNghieps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoSoTotNghieps_SinhViens_MaSinhVien",
                        column: x => x.MaSinhVien,
                        principalTable: "SinhViens",
                        principalColumn: "MaSinhVien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LienKetHoTros",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoaiHoTro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaSinhVien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaThamChieu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HeThongNguon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayLienKetUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LienKetHoTros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LienKetHoTros_SinhViens_MaSinhVien",
                        column: x => x.MaSinhVien,
                        principalTable: "SinhViens",
                        principalColumn: "MaSinhVien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LienKetTaiChinhs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoaiNghiaVu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaSinhVien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaThamChieu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HeThongNguon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayLienKetUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LienKetTaiChinhs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LienKetTaiChinhs_SinhViens_MaSinhVien",
                        column: x => x.MaSinhVien,
                        principalTable: "SinhViens",
                        principalColumn: "MaSinhVien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LanTheoHocs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaSinhVien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaChuongTrinh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    KhoaNhapHoc = table.Column<int>(type: "int", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LaChuongTrinhChinh = table.Column<bool>(type: "bit", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanTheoHocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LanTheoHocs_ChuongTrinhDaoTaos_MaChuongTrinh",
                        column: x => x.MaChuongTrinh,
                        principalTable: "ChuongTrinhDaoTaos",
                        principalColumn: "MaChuongTrinh",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LanTheoHocs_SinhViens_MaSinhVien",
                        column: x => x.MaSinhVien,
                        principalTable: "SinhViens",
                        principalColumn: "MaSinhVien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HoSoTotNghieps_MaSinhVien",
                table: "HoSoTotNghieps",
                column: "MaSinhVien");

            migrationBuilder.CreateIndex(
                name: "IX_LanTheoHocs_MaChuongTrinh",
                table: "LanTheoHocs",
                column: "MaChuongTrinh");

            migrationBuilder.CreateIndex(
                name: "IX_LanTheoHocs_MaSinhVien_MaChuongTrinh_NgayBatDau",
                table: "LanTheoHocs",
                columns: new[] { "MaSinhVien", "MaChuongTrinh", "NgayBatDau" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LienKetHoTros_MaSinhVien",
                table: "LienKetHoTros",
                column: "MaSinhVien");

            migrationBuilder.CreateIndex(
                name: "IX_LienKetTaiChinhs_MaSinhVien",
                table: "LienKetTaiChinhs",
                column: "MaSinhVien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoSoTotNghieps");

            migrationBuilder.DropTable(
                name: "LanTheoHocs");

            migrationBuilder.DropTable(
                name: "LienKetHoTros");

            migrationBuilder.DropTable(
                name: "LienKetTaiChinhs");

            migrationBuilder.DropTable(
                name: "ChuongTrinhDaoTaos");

            migrationBuilder.DropColumn(
                name: "NgayKiemTraHoSoUtc",
                table: "SinhViens");

            migrationBuilder.DropColumn(
                name: "NgayTiepNhanUtc",
                table: "SinhViens");

            migrationBuilder.DropColumn(
                name: "NguoiTiepNhanId",
                table: "SinhViens");

            migrationBuilder.DropColumn(
                name: "TrangThaiHoSo",
                table: "SinhViens");

            migrationBuilder.DropColumn(
                name: "NgayHieuLuc",
                table: "LichSuTrangThais");
        }
    }
}
