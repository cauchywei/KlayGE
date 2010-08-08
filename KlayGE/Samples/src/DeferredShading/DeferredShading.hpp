#ifndef _CARTOON_HPP
#define _CARTOON_HPP

#include <KlayGE/App3D.hpp>
#include <KlayGE/Font.hpp>
#include <KlayGE/CameraController.hpp>
#include <KlayGE/UI.hpp>

#include "DeferredShadingLayer.hpp"

class DeferredShadingApp : public KlayGE::App3DFramework
{
public:
	DeferredShadingApp();

private:
	void InitObjects();

	void DoUpdateOverlay();
	KlayGE::uint32_t DoUpdate(KlayGE::uint32_t pass);

	void OnResize(KlayGE::uint32_t width, KlayGE::uint32_t height);

	void InputHandler(KlayGE::InputEngine const & sender, KlayGE::InputAction const & action);
	void BufferChangedHandler(KlayGE::UIComboBox const & sender);
	void AntiAliasHandler(KlayGE::UICheckBox const & sender);
	void SSAOChangedHandler(KlayGE::UIComboBox const & sender);
	void CtrlCameraHandler(KlayGE::UICheckBox const & sender);

	KlayGE::FontPtr font_;
	KlayGE::RenderModelPtr scene_model_;
	std::vector<KlayGE::SceneObjectPtr> scene_objs_;
	KlayGE::SceneObjectPtr point_light_src_;
	KlayGE::SceneObjectPtr spot_light_src_[2];

	KlayGE::SceneObjectPtr sky_box_;

	KlayGE::FirstPersonCameraController fpcController_;

	KlayGE::DeferredShadingLayerPtr deferred_shading_;

	KlayGE::PostProcessPtr edge_anti_alias_;

	KlayGE::TexturePtr ssao_tex_;
	KlayGE::PostProcessPtr ssao_pp_;

	KlayGE::TexturePtr hdr_tex_;
	KlayGE::HDRPostProcessPtr hdr_pp_;

	KlayGE::PostProcessPtr debug_pp_;

	KlayGE::UIDialogPtr dialog_;
	int buffer_type_;
	bool anti_alias_enabled_;

	int id_buffer_combo_;
	int id_anti_alias_;
	int id_ssao_combo_;
	int id_ctrl_camera_;

	KlayGE::DeferredAmbientLightSourcePtr ambient_light_;
	KlayGE::DeferredPointLightSourcePtr point_light_;
	KlayGE::DeferredSpotLightSourcePtr spot_light_[2];

	size_t num_objs_rendered_;
	size_t num_renderable_rendered_;
	size_t num_primitives_rendered_;
	size_t num_vertices_rendered_;
};

#endif		// _CARTOON_HPP
