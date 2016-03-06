#include <KlayGE/KlayGE.hpp>
#include <KFL/Util.hpp>
#include <KlayGE/Texture.hpp>
#include <KFL/Math.hpp>
#include <KlayGE/ResLoader.hpp>
#include <KlayGE/TexCompression.hpp>
#include <KlayGE/TexCompressionBC.hpp>
#include <KlayGE/TexCompressionETC.hpp>

#include <boost/algorithm/string/case_conv.hpp>

#include <iostream>
#include <fstream>
#include <vector>

using namespace std;
using namespace KlayGE;

namespace
{
	void CompressTex(std::string const & in_file, std::string const & out_file, ElementFormat fmt)
	{
		Texture::TextureType in_type;
		uint32_t in_width, in_height, in_depth;
		uint32_t in_num_mipmaps;
		uint32_t in_array_size;
		ElementFormat in_format;
		std::vector<ElementInitData> in_data;
		std::vector<uint8_t> in_data_block;
		LoadTexture(in_file, in_type, in_width, in_height, in_depth, in_num_mipmaps, in_array_size, in_format, in_data, in_data_block);

		if (IsSigned(in_format))
		{
			fmt = MakeSigned(fmt);
		}
		if (IsSRGB(in_format))
		{
			fmt = MakeSRGB(fmt);
		}

		TexCompressionPtr in_codec;
		if (IsCompressedFormat(in_format))
		{
			switch (in_format)
			{
			case EF_BC1:
			case EF_BC1_SRGB:
			case EF_SIGNED_BC1:
				in_codec = MakeSharedPtr<TexCompressionBC1>();
				break;

			case EF_BC2:
			case EF_BC2_SRGB:
			case EF_SIGNED_BC2:
				in_codec = MakeSharedPtr<TexCompressionBC2>();
				break;

			case EF_BC3:
			case EF_BC3_SRGB:
			case EF_SIGNED_BC3:
				in_codec = MakeSharedPtr<TexCompressionBC3>();
				break;

			case EF_BC4:
			case EF_BC4_SRGB:
			case EF_SIGNED_BC4:
				in_codec = MakeSharedPtr<TexCompressionBC4>();
				break;

			case EF_BC5:
			case EF_BC5_SRGB:
			case EF_SIGNED_BC5:
				in_codec = MakeSharedPtr<TexCompressionBC5>();
				break;

			case EF_BC6:
				in_codec = MakeSharedPtr<TexCompressionBC6U>();
				break;

			case EF_SIGNED_BC6:
				in_codec = MakeSharedPtr<TexCompressionBC6S>();
				break;

			case EF_BC7:
			case EF_BC7_SRGB:
				in_codec = MakeSharedPtr<TexCompressionBC7>();
				break;

			case EF_ETC1:
				in_codec = MakeSharedPtr<TexCompressionETC1>();
				break;

			case EF_ETC2_BGR8:
			case EF_ETC2_BGR8_SRGB:
				in_codec = MakeSharedPtr<TexCompressionETC2RGB8>();
				break;

			case EF_ETC2_A1BGR8:
			case EF_ETC2_A1BGR8_SRGB:
				in_codec = MakeSharedPtr<TexCompressionETC2RGB8A1>();
				break;

			case EF_ETC2_ABGR8:
			case EF_ETC2_ABGR8_SRGB:
				// TODO
				BOOST_ASSERT(false);
				break;

			case EF_ETC2_R11:
			case EF_SIGNED_ETC2_R11:
				// TODO
				BOOST_ASSERT(false);
				break;

			case EF_ETC2_GR11:
			case EF_SIGNED_ETC2_GR11:
				// TODO
				BOOST_ASSERT(false);
				break;

			default:
				BOOST_ASSERT(false);
				break;
			}
		}

		TexCompressionPtr out_codec;
		switch (fmt)
		{
		case EF_BC1:
		case EF_BC1_SRGB:
		case EF_SIGNED_BC1:
			out_codec = MakeSharedPtr<TexCompressionBC1>();
			break;

		case EF_BC2:
		case EF_BC2_SRGB:
		case EF_SIGNED_BC2:
			out_codec = MakeSharedPtr<TexCompressionBC2>();
			break;

		case EF_BC3:
		case EF_BC3_SRGB:
		case EF_SIGNED_BC3:
			out_codec = MakeSharedPtr<TexCompressionBC3>();
			break;

		case EF_BC4:
		case EF_BC4_SRGB:
		case EF_SIGNED_BC4:
			out_codec = MakeSharedPtr<TexCompressionBC4>();
			break;

		case EF_BC5:
		case EF_BC5_SRGB:
		case EF_SIGNED_BC5:
			out_codec = MakeSharedPtr<TexCompressionBC5>();
			break;

		case EF_BC6:
			out_codec = MakeSharedPtr<TexCompressionBC6U>();
			break;

		case EF_SIGNED_BC6:
			out_codec = MakeSharedPtr<TexCompressionBC6S>();
			break;

		case EF_BC7:
		case EF_BC7_SRGB:
			out_codec = MakeSharedPtr<TexCompressionBC7>();
			break;

		case EF_ETC1:
			out_codec = MakeSharedPtr<TexCompressionETC1>();
			break;

		case EF_ETC2_BGR8:
		case EF_ETC2_BGR8_SRGB:
			out_codec = MakeSharedPtr<TexCompressionETC2RGB8>();
			break;

		case EF_ETC2_A1BGR8:
		case EF_ETC2_A1BGR8_SRGB:
			out_codec = MakeSharedPtr<TexCompressionETC2RGB8A1>();
			break;

		case EF_ETC2_ABGR8:
		case EF_ETC2_ABGR8_SRGB:
			// TODO
			BOOST_ASSERT(false);
			break;

		case EF_ETC2_R11:
		case EF_SIGNED_ETC2_R11:
			// TODO
			BOOST_ASSERT(false);
			break;

		case EF_ETC2_GR11:
		case EF_SIGNED_ETC2_GR11:
			// TODO
			BOOST_ASSERT(false);
			break;

		default:
			BOOST_ASSERT(false);
			break;
		}

		uint32_t out_width = (in_width + out_codec->BlockWidth()) & ~out_codec->BlockWidth();
		uint32_t out_height = (in_height + out_codec->BlockHeight()) & ~out_codec->BlockHeight();

		std::vector<ElementInitData> new_data(in_data.size());
		std::vector<std::vector<uint8_t>> new_data_block(in_data.size());

		std::vector<std::tuple<uint32_t, uint32_t, uint32_t>> block_addrs;
		for (uint32_t array_index = 0; array_index < in_array_size; ++ array_index)
		{
			uint32_t src_width = in_width;
			uint32_t src_height = in_height;

			uint32_t dst_width = out_width;
			uint32_t dst_height = out_height;

			for (uint32_t mip = 0; mip < in_num_mipmaps; ++ mip)
			{
				uint32_t const sub_res = array_index * in_num_mipmaps + mip;
				uint32_t const block_size = out_codec->BlockBytes();

				ElementInitData& dst_data = new_data[sub_res];

				dst_data.row_pitch = ((dst_width + out_codec->BlockWidth() - 1) / out_codec->BlockWidth()) * block_size;
				dst_data.slice_pitch = dst_data.row_pitch * ((dst_height + out_codec->BlockHeight() - 1) / out_codec->BlockHeight());

				new_data_block[sub_res].resize(dst_data.slice_pitch);
				dst_data.data = &new_data_block[sub_res][0];

				for (uint32_t y = 0; y < src_height; y += out_codec->BlockHeight())
				{
					for (uint32_t x = 0; x < src_width; x += out_codec->BlockWidth())
					{
						block_addrs.push_back(std::make_tuple(sub_res, x, y));
					}
				}

				src_width = std::max(src_width / 2, 1U);
				src_height = std::max(src_height / 2, 1U);

				dst_width = std::max(dst_width / 2, 1U);
				dst_height = std::max(dst_height / 2, 1U);
			}
		}

		size_t last_disp_num_blocks = 0;
		for (size_t i = 0; i < block_addrs.size(); ++ i)
		{
			if ((0 == last_disp_num_blocks) || (i == block_addrs.size() - 1) || (i - last_disp_num_blocks > 500))
			{
				cout.precision(2);
				cout << fixed << i * 100.0f / (block_addrs.size() - 1) << " %        \r";
				last_disp_num_blocks = i;
			}

			uint32_t const sub_res = std::get<0>(block_addrs[i]);
			uint32_t const x = std::get<1>(block_addrs[i]);
			uint32_t const y = std::get<2>(block_addrs[i]);
			uint32_t const array_index = sub_res / in_num_mipmaps;
			uint32_t const mip = sub_res - array_index * in_num_mipmaps;

			ElementInitData& sub_res_data = in_data[std::get<0>(block_addrs[i])];
			uint8_t const * src_data = static_cast<uint8_t const *>(sub_res_data.data);
			std::vector<uint8_t> block_in_data;
			if (in_codec)
			{
				BOOST_ASSERT(in_codec->BlockWidth() == out_codec->BlockWidth());
				BOOST_ASSERT(in_codec->BlockHeight() == out_codec->BlockHeight());

				block_in_data.resize(out_codec->BlockWidth() * out_codec->BlockHeight() * NumFormatBytes(in_codec->DecodedFormat()));
				in_codec->DecodeBlock(&block_in_data[0], src_data
					+ (y / in_codec->BlockHeight()) * sub_res_data.row_pitch + x / in_codec->BlockWidth() * in_codec->BlockBytes());
			}
			else
			{
				uint32_t elem_size = NumFormatBytes(in_format);
				block_in_data.resize(out_codec->BlockWidth() * out_codec->BlockHeight() * elem_size);
				for (uint32_t dy = 0; dy < out_codec->BlockHeight(); ++ dy)
				{
					if (y + dy < std::max(1U, in_height >> mip))
					{
						for (uint32_t dx = 0; dx < out_codec->BlockWidth(); ++ dx)
						{
							if (x + dx < std::max(1U, in_width >> mip))
							{
								memcpy(&block_in_data[(dy * out_codec->BlockWidth() + dx) * elem_size],
									src_data + (y + dy) * sub_res_data.row_pitch + (x + dx) * elem_size, elem_size);
							}
						}
					}
				}
			}

			ElementInitData& dst_data = new_data[sub_res];
			uint32_t const offset = y / out_codec->BlockHeight() * dst_data.row_pitch
				+ (x / out_codec->BlockWidth()) * out_codec->BlockBytes();
			out_codec->EncodeBlock(&new_data_block[sub_res][offset], &block_in_data[0], TCM_Quality);
		}

		SaveTexture(out_file, in_type, out_width, out_height, in_depth, in_num_mipmaps, in_array_size, fmt, new_data);

		cout << endl;
	}

	void PrintSupportedFormats()
	{
		cout << "Supported formats: bc1, bc2, bc3, bc4, bc5, bc7, etc1" << endl;
	}
}

int main(int argc, char* argv[])
{
	if (argc < 3)
	{
		cout << "Usage: TexCompressor format xxx.dds [yyy.dds]" << endl;
		cout << "\t";
		PrintSupportedFormats();
		return 1;
	}

	std::string fmt_str = argv[1];
	boost::algorithm::to_lower(fmt_str);
	size_t const fmt_hash = RT_HASH(fmt_str.c_str());

	ElementFormat fmt;
	if (CT_HASH("bc1") == fmt_hash)
	{
		fmt = EF_BC1;
	}
	else if (CT_HASH("bc2") == fmt_hash)
	{
		fmt = EF_BC2;
	}
	else if (CT_HASH("bc3") == fmt_hash)
	{
		fmt = EF_BC3;
	}
	else if (CT_HASH("bc4") == fmt_hash)
	{
		fmt = EF_BC4;
	}
	else if (CT_HASH("bc5") == fmt_hash)
	{
		fmt = EF_BC5;
	}
	else if (CT_HASH("bc7") == fmt_hash)
	{
		fmt = EF_BC7;
	}
	else if (CT_HASH("etc1") == fmt_hash)
	{
		fmt = EF_ETC1;
	}
	else
	{
		cout << "Unknown output format. ";
		PrintSupportedFormats();
		return 1;
	}

	std::string in_file = ResLoader::Instance().Locate(argv[2]);
	if (in_file.empty())
	{
		cout << "Couldn't locate " << in_file << endl;
		Context::Destroy();
		return 1;
	}

	std::string out_file;
	if (argc < 4)
	{
		out_file = in_file;
	}
	else
	{
		out_file = argv[3];
	}

	CompressTex(in_file, out_file, fmt);

	cout << "Compressed texture is saved." << endl;

	Context::Destroy();

	return 0;
}
